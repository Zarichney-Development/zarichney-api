using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Zarichney.Server.Cookbook.Orders;
using Zarichney.Server.Cookbook.Recipes;
using Zarichney.Server.Services;
using Zarichney.Server.Services.Emails;
using Zarichney.Server.Services.Sessions;

namespace Zarichney.Server.Controllers;

/// <summary>
/// Manages cookbook orders, recipes, and related processes like PDF generation and email notifications.
/// </summary>
/// <remarks>
/// This controller handles the creation and management of personalized cookbook orders.
/// It interacts with services for recipe handling, order processing, email validation and sending,
/// web scraping, background tasks, and session management.
/// Most endpoints require authentication via the cookie-based system established by AuthController.
/// </remarks>
[ApiController]
[Route("api")]
[Authorize] // Requires authentication for all actions in this controller
[Produces("application/json")] // Default content type for responses
public class CookbookController(
  IRecipeService recipeService,
  IOrderService orderService,
  IEmailService emailService,
  IBackgroundWorker worker,
  IRecipeRepository recipeRepository,
  WebScraperService scraperService,
  IScopeContainer scope,
  ISessionManager sessionManager,
  ILogger<CookbookController> logger
) : ControllerBase
{
  /// <summary>
  /// Provides a sample cookbook order submission structure.
  /// </summary>
  /// <remarks>
  /// Useful for front-end development to understand the expected format for creating a cookbook order.
  /// Returns a predefined `CookbookOrderSubmission` object.
  /// Does not actually create an order.
  /// </remarks>
  /// <returns>A sample `CookbookOrderSubmission` object.</returns>
  [HttpPost("cookbook/sample")]
  [ProducesResponseType(typeof(CookbookOrderSubmission), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult),
    StatusCodes.Status400BadRequest)] // Although unlikely in this simple case
  [SwaggerOperation(Summary = "Gets a sample cookbook order submission.",
    Description = "Returns a predefined example JSON structure for submitting a cookbook order.")]
  public IActionResult CreateSampleSubmission()
  {
    // Note: In a real scenario, this might fetch a template or default values.
    var submission = new CookbookOrderSubmission
    {
      Email = "user@example.com", // Example email
      CookbookContent = new CookbookContent
      {
        RecipeSpecificationType = "specific", // Or "criteria" etc. based on your models
        SpecificRecipes = ["Classic Beef Burger", "Chocolate Chip Cookies"], // Example recipes
        ExpectedRecipeCount = 5 // Example count
        // Add other relevant fields from CookbookContent
      }
      // Add other relevant fields from CookbookOrderSubmission
    };

    return Ok(submission);
  }

  /// <summary>
  /// Creates a new cookbook order based on user submission.
  /// </summary>
  /// <remarks>
  /// Receives the user's cookbook specifications, validates the email address,
  /// and initiates the order processing.
  /// The actual cookbook generation (finding recipes, compiling PDF) can be processed immediately or deferred
  /// based on the `processOrder` query parameter.
  /// </remarks>
  /// <param name="submission">The details for the cookbook order, including email and recipe preferences.</param>
  /// <param name="processOrder">If true (default), the order processing (e.g., recipe fetching, PDF generation) starts immediately (potentially in the background). If false, only the order record is created.</param>
  /// <returns>
  /// - 201 Created: Returns the created `CookbookOrder` object with its ID and sets the Location header.
  /// - 400 Bad Request: If the email is missing or invalid (based on `IEmailService.ValidateEmail`).
  /// - 500 Internal Server Error: If any other unexpected error occurs during processing.
  /// </returns>
  [HttpPost("cookbook")]
  [ProducesResponseType(typeof(CookbookOrder), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(BadRequestObjectResult),
    StatusCodes.Status400BadRequest)] // Covers missing email and InvalidEmailException
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Creates a new cookbook order.",
    Description =
      "Submits cookbook preferences, validates email, and creates an order record. Can optionally trigger immediate processing.")]
  public async Task<IActionResult> CreateCookbook([FromBody] CookbookOrderSubmission submission,
    [FromQuery] bool processOrder = true)
  {
    try
    {
      // reject if no email
      if (string.IsNullOrWhiteSpace(submission.Email))
      {
        logger.LogWarning("{Method}: No email provided in order", nameof(CreateCookbook));
        return BadRequest("Email is required"); // Simple string response for this specific validation
      }

      // Validate email format and potentially deliverability
      await emailService.ValidateEmail(submission.Email);

      // Process the submission to create an order record and potentially start background work
      var order = await orderService.ProcessSubmission(submission, processOrder);

      // Return 201 Created with the location of the new resource and the order object itself
      return Created($"/api/cookbook/order/{order.OrderId}", order);
    }
    catch (InvalidEmailException ex)
    {
      logger.LogWarning(ex, "{Method}: Invalid email validation for {Email}", nameof(CreateCookbook), submission.Email);
      // Return a structured error object for invalid email
      return BadRequest(new { error = ex.Message, email = ex.Email, reason = ex.Reason.ToString() });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to create cookbook", nameof(CreateCookbook));
      return new ApiErrorResult(ex, $"{nameof(CreateCookbook)}: Failed to create cookbook");
    }
  }

  /// <summary>
  /// Retrieves the details of a specific cookbook order.
  /// </summary>
  /// <remarks>
  /// Fetches the `CookbookOrder` by its unique ID.
  /// Also attempts to establish or retrieve a session associated with this order, setting an `X-Session-Id` header in the response.
  /// </remarks>
  /// <param name="orderId">The unique identifier of the cookbook order.</param>
  /// <returns>
  /// - 200 OK: Returns the `CookbookOrder` object. The `X-Session-Id` header is set.
  /// - 404 Not Found: If no order exists with the specified `orderId`.
  /// - 500 Internal Server Error: If any other unexpected error occurs.
  /// </returns>
  [HttpGet("cookbook/order/{orderId}")]
  [ProducesResponseType(typeof(CookbookOrder), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)] // Use NotFoundResult for consistency
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Gets cookbook order details.",
    Description =
      "Retrieves a specific cookbook order by ID and manages an associated session via the X-Session-Id header.")]
  public async Task<IActionResult> GetOrder([FromRoute] string orderId)
  {
    try
    {
      // Manage session associated with the order
      var session = await sessionManager.GetSessionByOrder(orderId, scope.Id);
      session.Duration ??= TimeSpan.FromMinutes(5); // Set default duration if needed
      session.ExpiresImmediately = false; // Ensure session persists
      Response.Headers.Append("X-Session-Id", session.Id.ToString()); // Set session ID header

      // Retrieve the order details
      var order = await orderService.GetOrder(orderId);

      return Ok(order);
    }
    catch (KeyNotFoundException ex) // Specific exception for order not found
    {
      logger.LogWarning(ex, "{Method}: Order not found: {OrderId}", nameof(GetOrder), orderId);
      return NotFound($"Order not found: {orderId}"); // Consistent message
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to retrieve order {OrderId}", nameof(GetOrder), orderId);
      return new ApiErrorResult(ex, $"{nameof(GetOrder)}: Failed to retrieve order");
    }
  }

  /// <summary>
  /// Triggers reprocessing for an existing cookbook order.
  /// </summary>
  /// <remarks>
  /// Re-queues the generation or processing steps for a given order ID.
  /// Useful if the initial processing failed or needs to be redone.
  /// The actual processing happens in a background task.
  /// The endpoint returns quickly after queuing the task.
  /// </remarks>
  /// <param name="orderId">The unique identifier of the cookbook order to reprocess.</param>
  /// <returns>
  /// - 200 OK: Returns the current `CookbookOrder` object immediately after queuing the background task.
  /// - 404 Not Found: If no order exists with the specified `orderId`.
  /// - 500 Internal Server Error: If queuing the background task or retrieving the order fails.
  /// </returns>
  [HttpPost("cookbook/order/{orderId}")]
  [ProducesResponseType(typeof(CookbookOrder), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)] // Use NotFoundResult
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Reprocesses a cookbook order.",
    Description = "Queues a background task to regenerate or reprocess an existing cookbook order.")]
  public async Task<IActionResult> ReprocessOrder([FromRoute] string orderId)
  {
    try
    {
      // Retrieve the order first to ensure it exists
      var order = await orderService.GetOrder(orderId);

      // Queue the background work using the worker service
      worker.QueueBackgroundWorkAsync(async (newScope, _) =>
      {
        // Resolve necessary services within the new scope for the background task
        var backgroundOrderService = newScope.GetService<IOrderService>();
        
        // Call the appropriate method to process the order
        await backgroundOrderService.ProcessOrder(orderId);
      });

      // Optional: Small delay to allow the background task to potentially start
      // Consider if this is truly necessary or if immediate return is better UX.
      await Task.Delay(500);

      // Return the current state of the order immediately
      return Ok(order);
    }
    catch (KeyNotFoundException ex) // Catch specific exception for order not found
    {
      logger.LogWarning(ex, "{Method}: Order not found for reprocessing: {OrderId}", nameof(ReprocessOrder), orderId);
      return NotFound($"Order not found: {orderId}"); // Consistent message
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to reprocess order {OrderId}", nameof(ReprocessOrder), orderId);
      return new ApiErrorResult(ex, $"{nameof(ReprocessOrder)}: Failed to reprocess order");
    }
  }

  /// <summary>
  /// Retrieves the generated cookbook PDF for a specific order.
  /// </summary>
  /// <remarks>
  /// Fetches the PDF file associated with the order.
  /// Optionally allows forcing a rebuild of the PDF before retrieval.
  /// Optionally allows emailing the PDF to the order's recipient after retrieval/rebuild.
  /// </remarks>
  /// <param name="orderId">The unique identifier of the cookbook order.</param>
  /// <param name="rebuild">If true, forces the PDF compilation process to run again before returning the file.</param>
  /// <param name="email">If true, sends the generated or retrieved PDF via email to the address associated with the order.</param>
  /// <returns>
  /// - 200 OK: Returns the PDF file (`application/pdf`).
  /// - 400 Bad Request: If the `orderId` parameter is missing or empty.
  /// - 404 Not Found: If the order or its associated PDF cannot be found.
  /// - 500 Internal Server Error: If PDF generation, retrieval, or emailing fails.
  /// </returns>
  [HttpGet("cookbook/order/{orderId}/pdf")]
  [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")] // Specify content type
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)] // Use NotFoundResult
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Gets the cookbook PDF.",
    Description = "Downloads the PDF for an order. Can optionally rebuild the PDF and/or email it.")]
  public async Task<IActionResult> GetCookbookPdf(
    [FromRoute] string orderId,
    [FromQuery] bool rebuild = false,
    [FromQuery] bool email = false)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(orderId))
      {
        logger.LogWarning("{Method}: Empty orderId received", nameof(GetCookbookPdf));
        return BadRequest("OrderId parameter is required"); // Simple string response
      }

      // Get order details (needed for email and potentially rebuild context)
      var order = await orderService.GetOrder(orderId); // Throws KeyNotFoundException if not found

      if (rebuild)
      {
        // Force compilation of the PDF
        await orderService.CompilePdf(order, true);
      }

      // Retrieve the PDF bytes (throws if PDF doesn't exist after compile/retrieve attempt)
      var pdfBytes = await orderService.GetPdf(orderId);

      if (email)
      {
        // Trigger emailing the cookbook PDF
        await orderService.EmailCookbook(order.OrderId); // Use orderId to ensure consistency
      }

      // Return the PDF file content
      return File(pdfBytes, "application/pdf", $"cookbook-{orderId}.pdf");
    }
    catch (KeyNotFoundException ex) // Catch order or PDF not found
    {
      logger.LogWarning(ex, "{Method}: Order or PDF not found for order: {OrderId}", nameof(GetCookbookPdf), orderId);
      // Provide a more specific message depending on whether it was the order or the PDF, if possible
      return NotFound($"Order or associated PDF not found: {orderId}");
    }
    // Catch specific exceptions from CompilePdf or GetPdf if they are defined and informative
    // catch (PdfGenerationException ex) { ... }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to get or process PDF for order {OrderId}", nameof(GetCookbookPdf),
        orderId);
      return new ApiErrorResult(ex, $"{nameof(GetCookbookPdf)}: Failed to get or process PDF");
    }
  }

  /// <summary>
  /// Resends the cookbook PDF via email for a specific order.
  /// </summary>
  /// <remarks>
  /// Triggers the email service to send the cookbook PDF associated with the given order ID
  /// to the recipient email address stored in the order details. Assumes the PDF already exists.
  /// </remarks>
  /// <param name="orderId">The unique identifier of the cookbook order.</param>
  /// <returns>
  /// - 200 OK: Returns a success message indicating the email has been queued for sending.
  /// - 404 Not Found: If the order specified by `orderId` does not exist.
  /// - 500 Internal Server Error: If there's an issue retrieving order details or sending the email.
  /// </returns>
  [HttpPost("cookbook/order/{orderId}/email")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)] // Simple string success message
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)] // Use NotFoundResult
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Resends the cookbook email.",
    Description = "Triggers sending the cookbook PDF via email for a specified order.")]
  public async Task<IActionResult> ResendCookbook([FromRoute] string orderId)
  {
    try
    {
      // The EmailCookbook service method should handle fetching the order and PDF internally.
      // It should throw KeyNotFoundException if the order or PDF doesn't exist.
      await orderService.EmailCookbook(orderId);
      return Ok("Cookbook email has been queued for sending."); // More informative success message
    }
    catch (KeyNotFoundException ex) // Handles order or potentially PDF not found within EmailCookbook
    {
      logger.LogWarning(ex, "{Method}: Order not found for email resend: {OrderId}", nameof(ResendCookbook), orderId);
      return NotFound($"Order not found: {orderId}"); // Consistent message
    }
    // Catch specific email sending exceptions if IEmailService defines them
    // catch (EmailSendingFailedException ex) { ... return Problem(...) }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to resend email for order {OrderId}", nameof(ResendCookbook), orderId);
      return new ApiErrorResult(ex, $"{nameof(ResendCookbook)}: Failed to resend cookbook email");
    }
  }

  /// <summary>
  /// Searches for recipes based on a query string.
  /// </summary>
  /// <remarks>
  /// Queries the existing recipe database. Optionally, can trigger a web scrape if no suitable recipes are found locally.
  /// Allows specifying criteria like minimum score and desired number of recipes.
  /// </remarks>
  /// <param name="query">The search term for recipes (e.g., "chicken soup", "vegan chocolate cake").</param>
  /// <param name="scrape">If true, performs a web scrape if the initial database search yields insufficient results based on other parameters.</param>
  /// <param name="acceptableScore">Optional minimum score threshold for recipes to be considered acceptable.</param>
  /// <param name="requiredCount">Optional desired number of recipes to return.</param>
  /// <returns>
  /// - 200 OK: Returns a list of `Recipe` objects matching the query.
  /// - 400 Bad Request: If the `query` parameter is missing or empty.
  /// - 404 Not Found: If no recipes are found matching the criteria (after potential scraping).
  /// - 500 Internal Server Error: If an unexpected error occurs during search or scraping.
  /// </returns>
  [HttpGet("recipe")]
  [ProducesResponseType(typeof(IEnumerable<Recipe>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(NotFoundResult),
    StatusCodes.Status404NotFound)] // Covers NoRecipeException and empty results
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Searches for recipes.",
    Description = "Finds recipes by query. Can optionally scrape web sources if local results are insufficient.")]
  public async Task<IActionResult> GetRecipes(
    [FromQuery] string query,
    [FromQuery] bool scrape = false,
    [FromQuery] int? acceptableScore = null,
    [FromQuery] int? requiredCount = null
  )
  {
    try
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        logger.LogWarning("{Method}: Empty query received", nameof(GetRecipes));
        return BadRequest("Query parameter is required"); // Simple string response
      }

      // Delegate complex logic of searching and conditional scraping to the service
      var recipes = await recipeService.GetRecipes(query, scrape, acceptableScore, requiredCount);

      // Check if the result list is empty *after* the service call (which might include scraping)
      if (recipes.Count == 0)
      {
        // Use NotFound Result for consistency
        return NotFound($"No recipes found matching the criteria for '{query}'");
      }

      return Ok(recipes);
    }
    catch (NoRecipeException e) // Specific exception from the service layer
    {
      logger.LogWarning(e, "{Method}: No recipes found exception for query: {Query}", nameof(GetRecipes), query);
      return NotFound(e.Message); // Return the exception message from the service
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to get recipes for query: {Query}", nameof(GetRecipes), query);
      return new ApiErrorResult(ex, $"{nameof(GetRecipes)}: Failed to retrieve recipes");
    }
  }


  /// <summary>
  /// Scrapes web sources for recipes based on a query.
  /// </summary>
  /// <remarks>
  /// Directly initiates a web scraping process to find recipes matching the query.
  /// Can optionally filter by a specific website, set score thresholds, and specify the number needed.
  /// Optionally stores the found and ranked recipes in the repository.
  /// </remarks>
  /// <param name="query">The search term for recipes.</param>
  /// <param name="site">Optional: Restrict scraping to a specific domain (e.g., "allrecipes.com").</param>
  /// <param name="acceptableScore">Optional minimum score threshold for scraped recipes.</param>
  /// <param name="recipesNeeded">Optional target number of recipes to find.</param>
  /// <param name="store">If true, ranks and stores the newly scraped recipes in the database asynchronously.</param>
  /// <returns>
  /// - 200 OK: Returns a list of `Recipe` objects found via scraping. If `store` is true, returns the newly ranked recipes intended for storage.
  /// - 400 Bad Request: If the `query` parameter is missing or empty.
  /// - 404 Not Found: If the scraping process yields no recipes matching the criteria.
  /// - 500 Internal Server Error: If an error occurs during scraping, ranking, or storing.
  /// </returns>
  [HttpGet("recipe/scrape")]
  [ProducesResponseType(typeof(IEnumerable<Recipe>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)] // If scraping yields nothing
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  [SwaggerOperation(Summary = "Scrapes web for recipes.",
    Description = "Directly scrapes web sources for recipes based on a query. Can optionally store results.")]
  public async Task<IActionResult> ScrapeRecipes(
    [FromQuery] string query,
    [FromQuery] string? site = null,
    [FromQuery] int? acceptableScore = null,
    [FromQuery] int? recipesNeeded = null,
    [FromQuery] bool? store = false) // Nullable bool just in case
  {
    try
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        logger.LogWarning("{Method}: Empty query received", nameof(ScrapeRecipes));
        return BadRequest("Query parameter is required"); // Simple string response
      }

      // Perform scraping using the dedicated service
      var recipes = await scraperService.ScrapeForRecipesAsync(query, acceptableScore, recipesNeeded, site);

      // Check if scraping returned any recipes
      if (recipes.Count == 0)
      {
        return NotFound($"No recipes found via scraping for '{query}'" + (site != null ? $" on site '{site}'" : ""));
      }

      // If not storing, return the raw scraped recipes
      if (store != true)
      {
        return Ok(recipes);
      }

      // --- Processing for Ranking and Storing ---

      // Filter out recipes already present in the repository before ranking
      // Assuming ContainsRecipe checks by a unique ID (e.g., URL or generated hash)
      var recipesToRank =
        recipes.Where(r => r.Id != null && !recipeRepository.ContainsRecipe(r.Id)).ToList(); // Ensure ID is not null

      if (recipesToRank.Count == 0)
      {
        logger.LogInformation(
          "{Method}: Scraped recipes for '{Query}' already exist in repository. Nothing new to rank or store.",
          nameof(ScrapeRecipes), query);
        // Return the original scraped list, or perhaps an empty list, or the existing ones? Decide based on desired UX.
        // Returning the original full list might be confusing if store=true was requested. Let's return the empty list of *new* recipes.
        return Ok(new List<Recipe>()); // No *new* recipes to process/return
      }

      // Rank the *new* recipes using the recipe service
      var rankedNewRecipes = await recipeService.RankUnrankedRecipesAsync(recipesToRank, query);

      // Asynchronously add/update the newly ranked recipes to the repository
      // No need to await this; let it run in the background.
      recipeRepository.AddUpdateRecipesAsync(rankedNewRecipes);

      // Return the list of recipes that were ranked and are being stored.
      return Ok(rankedNewRecipes);
    }
    catch (Exception ex) // Catch potential errors from scraping, ranking, or repository interaction
    {
      logger.LogError(ex, "{Method}: Failed to scrape or process recipes for query: {Query}", nameof(ScrapeRecipes),
        query);
      return new ApiErrorResult(ex, $"{nameof(ScrapeRecipes)}: Failed to scrape or process recipes");
    }
  }
}