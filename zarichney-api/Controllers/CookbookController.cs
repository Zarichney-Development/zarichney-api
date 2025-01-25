using Microsoft.AspNetCore.Mvc;
using Zarichney.Cookbook.Orders;
using Zarichney.Cookbook.Recipes;
using Zarichney.Middleware;
using Zarichney.Services;
using Zarichney.Services.Emails;
using Zarichney.Services.Sessions;

namespace Zarichney.Controllers;

[ApiController]
[Route("api")]
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
  [HttpPost("cookbook/sample")]
  [ProducesResponseType<CookbookOrderSubmission>(StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  public IActionResult CreateSampleSubmission()
  {
    
    var submission = new CookbookOrderSubmission
    {
      Email = "zarichney@gmail.com",
      CookbookContent = new CookbookContent
      {
        RecipeSpecificationType = "specific",
        SpecificRecipes = ["Bacon Cheese burger"],
        ExpectedRecipeCount = 1
      }
    };
    
    return Ok(submission);
  }

  [HttpPost("cookbook")]
  [ProducesResponseType(typeof(CookbookOrder), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> CreateCookbook([FromBody] CookbookOrderSubmission submission,
    [FromQuery] bool processOrder = true)
  {
    try
    {
      // reject if no email
      if (string.IsNullOrWhiteSpace(submission.Email))
      {
        logger.LogWarning("{Method}: No email provided in order", nameof(CreateCookbook));
        return BadRequest("Email is required");
      }

      await emailService.ValidateEmail(submission.Email);
      var order = await orderService.ProcessSubmission(submission, processOrder);

      return Created($"/api/cookbook/order/{order.OrderId}", order);
    }
    catch (InvalidEmailException ex)
    {
      logger.LogWarning(ex, "{Method}: Invalid email validation for {Email}",
        nameof(CreateCookbook), submission.Email);
      return BadRequest(new { error = ex.Message, email = ex.Email, reason = ex.Reason.ToString() });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to create cookbook", nameof(CreateCookbook));
      return new ApiErrorResult(ex, $"{nameof(CreateCookbook)}: Failed to create cookbook");
    }
  }

  [HttpGet("cookbook/order/{orderId}")]
  [AcceptsSession]
  [ProducesResponseType(typeof(CookbookOrder), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetOrder([FromRoute] string orderId)
  {
    try
    {
      var session = await sessionManager.GetSessionByOrder(orderId, scope.Id);
      session.Duration ??= TimeSpan.FromMinutes(5);
      session.ExpiresImmediately = false;
      Response.Headers["X-Session-Id"] = session.Id.ToString();

      var order = await orderService.GetOrder(orderId);
      
      return Ok(order);
    }
    catch (KeyNotFoundException ex)
    {
      logger.LogWarning(ex, "{Method}: Order not found: {OrderId}", nameof(GetOrder), orderId);
      return NotFound($"Order not found: {orderId}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to retrieve order {OrderId}", nameof(GetOrder), orderId);
      return new ApiErrorResult(ex, $"{nameof(GetOrder)}: Failed to retrieve order");
    }
  }

  [HttpPost("cookbook/order/{orderId}")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public IActionResult ReprocessOrder([FromRoute] string orderId)
  {
    try
    {
      // TODO: add order Id validation and throw KeyNotFoundException if not found

      // Queue the cookbook generation task
      worker.QueueBackgroundWorkAsync(async (newScope, _) =>
      {
        var backgroundOrderService = newScope.GetService<IOrderService>();
        await backgroundOrderService.ProcessOrder(orderId);
      });

      return Ok("Reprocessing order");
    }
    catch (KeyNotFoundException ex)
    {
      logger.LogWarning(ex, "{Method}: Order not found for reprocessing: {OrderId}",
        nameof(ReprocessOrder), orderId);
      return NotFound($"Order not found: {orderId}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to reprocess order {OrderId}",
        nameof(ReprocessOrder), orderId);
      return new ApiErrorResult(ex, $"{nameof(ReprocessOrder)}: Failed to reprocess order");
    }
  }

  [HttpPost("cookbook/order/{orderId}/pdf")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> RebuildPdf(
    [FromRoute] string orderId,
    [FromQuery] bool email = false)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(orderId))
      {
        logger.LogWarning("{Method}: Empty orderId received", nameof(RebuildPdf));
        return BadRequest("OrderId parameter is required");
      }

      var order = await orderService.GetOrder(orderId);

      await orderService.CompilePdf(order, email);

      if (email)
      {
        await orderService.EmailCookbook(order.OrderId);
        return Ok("PDF rebuilt and email sent");
      }

      return Ok("PDF rebuilt");
    }
    catch (KeyNotFoundException ex)
    {
      logger.LogWarning(ex, "{Method}: Order not found for PDF rebuild: {OrderId}",
        nameof(RebuildPdf), orderId);
      return NotFound($"Order not found: {orderId}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to rebuild PDF for order {OrderId}",
        nameof(RebuildPdf), orderId);
      return new ApiErrorResult(ex, $"{nameof(RebuildPdf)}: Failed to rebuild PDF");
    }
  }

  [HttpPost("cookbook/order/{orderId}/email")]
  [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ResendCookbook([FromRoute] string orderId)
  {
    try
    {
      await orderService.EmailCookbook(orderId);
      return Ok("Email sent");
    }
    catch (KeyNotFoundException ex)
    {
      logger.LogWarning(ex, "{Method}: Order not found for email resend: {OrderId}",
        nameof(ResendCookbook), orderId);
      return NotFound($"Order not found: {orderId}");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to resend email for order {OrderId}",
        nameof(ResendCookbook), orderId);
      return new ApiErrorResult(ex, $"{nameof(ResendCookbook)}: Failed to resend cookbook email");
    }
  }

  [HttpGet("recipe")]
  [ProducesResponseType(typeof(IEnumerable<Recipe>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
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
        return BadRequest("Query parameter is required");
      }

      var recipes = await recipeService.GetRecipes(query, scrape, acceptableScore, requiredCount);

      if (recipes.ToList().Count == 0)
      {
        return NotFound($"No recipes found for '{query}'");
      }

      return Ok(recipes);
    }
    catch (NoRecipeException e)
    {
      return NotFound(e.Message);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to get recipes for query: {Query}",
        nameof(GetRecipes), query);
      return new ApiErrorResult(ex, $"{nameof(GetRecipes)}: Failed to retrieve recipes");
    }
  }

  [HttpGet("recipe/scrape")]
  [ProducesResponseType(typeof(IEnumerable<Recipe>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(BadRequestObjectResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ScrapeRecipes(
    [FromQuery] string query,
    [FromQuery] string? site = null,
    [FromQuery] int? acceptableScore = null,
    [FromQuery] int? recipesNeeded = null,
    [FromQuery] bool? store = false)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(query))
      {
        logger.LogWarning("{Method}: Empty query received", nameof(ScrapeRecipes));
        return BadRequest("Query parameter is required");
      }

      var recipes = await scraperService.ScrapeForRecipesAsync(query, acceptableScore, recipesNeeded, site);

      if (recipes.ToList().Count == 0)
      {
        return NotFound($"No recipes found for '{query}'");
      }

      if (store != true)
      {
        return Ok(recipes);
      }

      // Further processing for ranking and storing recipes

      var newRecipes =
        await recipeService.RankUnrankedRecipesAsync(
          recipes.Where(r => !recipeRepository.ContainsRecipe(r.Id!)), query);

      // Process in the background
      recipeRepository.AddUpdateRecipesAsync(newRecipes);

      return Ok(newRecipes);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "{Method}: Failed to scrape recipes for query: {Query}",
        nameof(ScrapeRecipes), query);
      return new ApiErrorResult(ex, $"{nameof(ScrapeRecipes)}: Failed to scrape recipes");
    }
  }
}