using Microsoft.AspNetCore.Mvc;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Middleware;
using Zarichney.Services.Payment;

namespace Zarichney.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController(
  ILogger<PaymentController> logger,
  IPaymentService paymentService,
  IOrderService orderService,
  ICustomerService customerService,
  PaymentConfig config)
  : ControllerBase
{
  /// <summary>
  /// Creates a Stripe checkout session for the specified order
  /// </summary>
  /// <param name="orderId">Order identifier</param>
  /// <returns>The Stripe checkout session ID</returns>
  [HttpPost("create-checkout-session/{orderId}")]
  [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> CreateCheckoutSession([FromRoute] string orderId)
  {
    try
    {
      var order = await orderService.GetOrder(orderId);

      if (!order.RequiresPayment)
      {
        return BadRequest("Order does not require payment");
      }

      // Create Stripe checkout session
      var sessionId = await paymentService.CreateCheckoutSession(order);

      // Return the session ID to the client
      return Ok(new SessionResponse { SessionId = sessionId, PublicKey = config.StripePublishableKey });
    }
    catch (KeyNotFoundException ex)
    {
      logger.LogWarning(ex, "Order not found: {OrderId}", orderId);
      return NotFound($"Order {orderId} not found");
    }
    catch (InvalidOperationException ex)
    {
      logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating checkout session for order {OrderId}", orderId);
      return StatusCode(StatusCodes.Status500InternalServerError,
        "An error occurred while creating the checkout session");
    }
  }

  /// <summary>
  /// Creates a Stripe checkout session for purchasing recipe credits
  /// </summary>
  /// <returns>The Stripe checkout session ID</returns>
  [HttpPost("create-credit-session")]
  [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> CreateCreditSession([FromBody] RecipeCreditRequest request)
  {
    try
    {
      if (string.IsNullOrEmpty(request.Email))
      {
        return BadRequest("Email is required");
      }

      if (request.RecipeCount <= 0)
      {
        return BadRequest("Recipe count must be greater than zero");
      }

      // Get or create the customer
      var customer = await customerService.GetOrCreateCustomer(request.Email);

      // Create Stripe checkout session
      var sessionId = await paymentService.CreateCheckoutSession(customer, request.RecipeCount);

      // Return the session ID to the client
      return Ok(new SessionResponse { SessionId = sessionId, PublicKey = config.StripePublishableKey });
    }
    catch (ArgumentException ex)
    {
      logger.LogWarning(ex, "Invalid request: {Message}", ex.Message);
      return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating credit checkout session for {Email}", request.Email);
      return StatusCode(StatusCodes.Status500InternalServerError,
        "An error occurred while creating the checkout session");
    }
  }

  /// <summary>
  /// Handles Stripe webhook events
  /// </summary>
  /// <returns>An empty 200 OK response to acknowledge receipt of the webhook</returns>
  [HttpPost("webhook")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> HandleWebhook()
  {
    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

    try
    {
      // Get the Stripe-Signature header
      if (!Request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature))
      {
        logger.LogWarning("Webhook received without Stripe-Signature header");
        return BadRequest();
      }

      Request.Headers.TryGetValue("Stripe-Event-ID", out var eventId);

      // Check for idempotency - Stripe may send the same event multiple times
      if (Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey) ||
          !string.IsNullOrEmpty(eventId))
      {
        var key = !string.IsNullOrEmpty(idempotencyKey) ? idempotencyKey.ToString() : eventId.ToString();
        logger.LogInformation("Processing webhook with idempotency key: {Key}", key);

        // Here you could implement caching to check if this event was already processed
        // For now, we'll just log it and continue
      }

      // Process the webhook event
      await paymentService.HandleWebhookEvent(json, stripeSignature!);

      // Return a 200 OK response to acknowledge receipt of the webhook
      return Ok();
    }
    catch (Stripe.StripeException ex)
    {
      logger.LogError(ex, "Stripe error handling webhook: {Message}", ex.Message);

      // Always return 200 OK for webhook signature verification errors
      // This prevents Stripe from retrying webhooks that will always fail due to invalid signatures
      if (ex.StripeError?.Type == "signature_verification_failure")
      {
        return Ok("Signature verification failed, but acknowledging receipt");
      }

      return BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error handling webhook");
      return BadRequest(new { error = "Unexpected error processing webhook" });
    }
  }

  /// <summary>
  /// Gets information about a checkout session
  /// </summary>
  /// <param name="sessionId">The Stripe checkout session ID</param>
  /// <returns>Information about the checkout session</returns>
  [HttpGet("session/{sessionId}")]
  [ProducesResponseType(typeof(CheckoutSessionInfo), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ApiErrorResult), StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> GetSessionInfo([FromRoute] string sessionId)
  {
    try
    {
      var sessionInfo = await paymentService.GetSessionInfo(sessionId);
      return Ok(sessionInfo);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving session info for {SessionId}", sessionId);
      return StatusCode(StatusCodes.Status500InternalServerError,
        "An error occurred while retrieving the session information");
    }
  }
}

public class SessionResponse
{
  public string SessionId { get; init; } = string.Empty;
  public string PublicKey { get; init; } = string.Empty;
}

public class RecipeCreditRequest
{
  public string Email { get; init; } = string.Empty;
  public int RecipeCount { get; init; }
}