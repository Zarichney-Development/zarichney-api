using Zarichney.Server.Config;
using Zarichney.Server.Cookbook.Customers;
using Zarichney.Server.Cookbook.Orders;
using Customer = Zarichney.Server.Cookbook.Customers.Customer;

namespace Zarichney.Server.Services.Payment;

/// <summary>
/// Payment service for handling checkout sessions and webhooks.
/// </summary>
public interface IPaymentService
{
  /// <summary>
  /// Creates a checkout session for completing a specific order.
  /// </summary>
  Task<string> CreateCheckoutSession(CookbookOrder order);

  /// <summary>
  /// Creates a checkout session for purchasing recipe credits.
  /// </summary>
  Task<string> CreateCheckoutSession(Customer customer, int recipeCount);

  /// <summary>
  /// Gets information about a checkout session.
  /// </summary>
  Task<CheckoutSessionInfo> GetSessionInfo(string sessionId);

  /// <summary>
  /// Handles payment provider webhook events.
  /// </summary>
  Task HandleWebhookEvent(string requestBody, string signature);
}

/// <summary>
/// Service for handling payments and related operations.
/// </summary>
public class PaymentService(
  ILogger<PaymentService> logger,
  PaymentConfig config,
  ClientConfig clientConfig,
  IOrderService orderService,
  IOrderRepository orderRepository, // todo fix this: we cant have a singleton injected into a transient. Consider redesign the architecture
  ICustomerService customerService,
  IStripeService stripeService)
  : IPaymentService
{
  // Constants for Stripe event types.
  private const string CheckoutSessionCompletedEvent = "checkout.session.completed";
  private const string CheckoutSessionAsyncPaymentSucceededEvent = "checkout.session.async_payment_succeeded";
  private const string PaymentIntentSucceededEvent = "payment_intent.succeeded";
  private const string ChargeSucceededEvent = "charge.succeeded";
  private const string CheckoutSessionAsyncPaymentFailedEvent = "checkout.session.async_payment_failed";

  /// <summary>
  /// Creates a checkout session for completing a specific order.
  /// </summary>
  public async Task<string> CreateCheckoutSession(CookbookOrder order)
  {
    ArgumentNullException.ThrowIfNull(order);

    // Calculate how many more recipes the user needs to pay for.
    var recipesToPay = order.RecipeList.Count - order.SynthesizedRecipes.Count;

    if (recipesToPay <= 0)
    {
      throw new InvalidOperationException("This order doesn't require any payment.");
    }

    var metadata = new Dictionary<string, string>
    {
      { "payment_type", PaymentType.OrderCompletion.ToString() },
      { "order_id", order.OrderId },
      { "customer_email", order.Email }
    };

    var successUrl = $"{clientConfig.BaseUrl}{string.Format(config.SuccessUrl, order.OrderId)}";
    var cancelUrl = $"{clientConfig.BaseUrl}{string.Format(config.CancelUrl, order.OrderId)}";

    const string productName = "Cookbook Recipe";
    var productDescription = $"Additional recipes for your cookbook (needed: {recipesToPay})";

    var sessionId = await stripeService.CreateCheckoutSession(
      order.Email,
      recipesToPay,
      metadata,
      successUrl,
      cancelUrl,
      productName,
      productDescription,
      order.OrderId);

    logger.LogInformation("Created Stripe checkout session {SessionId} for order {OrderId}", sessionId, order.OrderId);
    return sessionId;
  }

  /// <summary>
  /// Creates a checkout session for purchasing recipe credits for a customer (no order associated).
  /// </summary>
  public async Task<string> CreateCheckoutSession(Customer customer, int recipeCount)
  {
    ArgumentNullException.ThrowIfNull(customer);

    if (recipeCount <= 0)
    {
      throw new ArgumentException("Recipe count must be greater than zero", nameof(recipeCount));
    }

    // Validate the recipe count is one of the allowed package sizes.
    if (!config.RecipePackageSizes.Contains(recipeCount))
    {
      recipeCount = config.RecipePackageSizes.OrderBy(size => Math.Abs(size - recipeCount)).First();
      logger.LogInformation("Adjusted recipe count to nearest package size: {RecipeCount}", recipeCount);
    }

    var metadata = new Dictionary<string, string>
    {
      { "payment_type", PaymentType.RecipeCredit.ToString() },
      { "customer_email", customer.Email },
      { "recipe_count", recipeCount.ToString() }
    };

    var successUrl = $"{clientConfig.BaseUrl}/account?success=true";
    var cancelUrl = $"{clientConfig.BaseUrl}/account?cancelled=true";

    var productName = "Recipe Credit";
    var productDescription = $"{recipeCount} recipe credits for your cookbook account";

    var sessionId = await stripeService.CreateCheckoutSession(
      customer.Email,
      recipeCount,
      metadata,
      successUrl,
      cancelUrl,
      productName,
      productDescription);

    logger.LogInformation("Created Stripe checkout session {SessionId} for customer {Email} to purchase {RecipeCount} credits",
      sessionId, customer.Email, recipeCount);
    return sessionId;
  }

  /// <summary>
  /// Gets information about a checkout session.
  /// </summary>
  public async Task<CheckoutSessionInfo> GetSessionInfo(string sessionId)
  {
    var session = await stripeService.GetSession(sessionId);

    return new CheckoutSessionInfo
    {
      Id = session.Id,
      Status = session.Status,
      CustomerEmail = session.CustomerEmail,
      AmountTotal = session.AmountTotal / 100m, // Convert from cents to dollars
      Currency = session.Currency,
      PaymentStatus = session.PaymentStatus
    };
  }

  /// <summary>
  /// Handles webhook events from Stripe.
  /// </summary>
  public async Task HandleWebhookEvent(string requestBody, string signature)
  {
    try
    {
      // Validate and parse the event.
      var stripeEvent = stripeService.ConstructEvent(requestBody, signature, config.StripeWebhookSecret);

      logger.LogInformation("Received Stripe webhook event of type: {EventType} with ID: {EventId}",
        stripeEvent.Type, stripeEvent.Id);

      // Handle the event based on its type.
      switch (stripeEvent.Type)
      {
        case CheckoutSessionCompletedEvent:
        case CheckoutSessionAsyncPaymentSucceededEvent:
          await HandleCheckoutSessionCompleted(stripeEvent);
          break;

        case PaymentIntentSucceededEvent:
          await HandlePaymentIntentSucceeded(stripeEvent);
          break;

        case ChargeSucceededEvent:
          await HandleChargeSucceeded(stripeEvent);
          break;

        case CheckoutSessionAsyncPaymentFailedEvent:
          await HandleAsyncPaymentFailed(stripeEvent);
          break;

        // Monitor but don't act on these events.
        case "payment_intent.created":
        case "payment_intent.requires_action":
        case "charge.updated":
        case "payment_method.attached":
          logger.LogInformation("Monitored Stripe event received: {EventType} with ID: {EventId}",
            stripeEvent.Type, stripeEvent.Id);
          break;

        default:
          logger.LogInformation("Unhandled Stripe event type: {EventType} with ID: {EventId}",
            stripeEvent.Type, stripeEvent.Id);
          break;
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error handling Stripe webhook: {Message}", ex.Message);
      throw;
    }
  }

  /// <summary>
  /// Handles completed checkout session events.
  /// </summary>
  private async Task HandleCheckoutSessionCompleted(dynamic stripeEvent)
  {
    try
    {
      var session = stripeEvent.Data.Object;
      var sessionId = session.Id as string ?? "unknown";

      // Get the complete session with expanded line items.
      var sessionWithLineItems = await stripeService.GetSessionWithLineItems(sessionId);
      var metadata = stripeService.ParseSessionMetadata(sessionWithLineItems);
      var customerEmail = metadata.CustomerEmail;
      var specificRecipeCount = metadata.RecipeCount;

      if (string.IsNullOrEmpty(customerEmail))
      {
        logger.LogWarning("Session {SessionId} has no customer email.", sessionId);
        return;
      }

      // Delegate quantity calculation to StripeService.
      var purchasedQuantity = stripeService.GetPurchasedQuantity(sessionWithLineItems);
      if (purchasedQuantity <= 0 && specificRecipeCount.HasValue)
      {
        purchasedQuantity = specificRecipeCount.Value;
        logger.LogInformation("Using metadata recipe count as purchased quantity: {Quantity}", purchasedQuantity);
      }

      if (purchasedQuantity <= 0)
      {
        logger.LogWarning("Session {SessionId} has no valid quantity", sessionId);
        return;
      }

      // Process based on payment type.
      switch (metadata.PaymentType)
      {
        case PaymentType.OrderCompletion:
          await ProcessOrderCompletion(sessionWithLineItems, customerEmail, purchasedQuantity);
          break;

        case PaymentType.RecipeCredit:
          await ProcessCreditPurchase(customerEmail, purchasedQuantity);
          break;

        default:
          logger.LogWarning("Unknown payment type: {PaymentType}", metadata.PaymentType);
          break;
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing checkout session completion");
    }
  }

  /// <summary>
  /// Processes order completion payments.
  /// </summary>
  private async Task ProcessOrderCompletion(dynamic session, string customerEmail, int purchasedQuantity)
  {
    var orderId = session.ClientReferenceId as string;
    var sessionIdStr = session.Id as string ?? "unknown";

    if (string.IsNullOrEmpty(orderId))
    {
      logger.LogWarning("Order completion session {SessionId} has no client_reference_id", sessionIdStr);
      return;
    }

    logger.LogInformation("Processing order completion payment for order {OrderId}, session {SessionId}",
      orderId, sessionIdStr);

    try
    {
      var order = await orderService.GetOrder(orderId);

      // Validate the order belongs to this customer.
      if (!string.Equals(order.Email, customerEmail, StringComparison.OrdinalIgnoreCase))
      {
        logger.LogWarning("Order {OrderId} email {OrderEmail} doesn't match payment email {PaymentEmail}",
          orderId, order.Email, customerEmail);
        // Continue processing since the payment was successful.
      }

      if (purchasedQuantity <= 0)
      {
        purchasedQuantity = order.RecipeList.Count - order.SynthesizedRecipes.Count;
        logger.LogInformation("Using recipe difference as purchased quantity: {Quantity}", purchasedQuantity);
      }

      if (purchasedQuantity <= 0)
      {
        logger.LogWarning("Order {OrderId} doesn't need any additional recipes", orderId);
        return;
      }

      await customerService.AddRecipes(order.Customer, purchasedQuantity);

      if (order.Status == OrderStatus.AwaitingPayment)
      {
        order.Status = OrderStatus.Paid;
      }

      order.RequiresPayment = false;
      orderService.QueueOrderProcessing(orderId);

      logger.LogInformation("Payment successful for order {OrderId}. Added {Quantity} recipes to customer {Email}.",
        orderId, purchasedQuantity, order.Customer.Email);
    }
    catch (KeyNotFoundException ex)
    {
      logger.LogError(ex, "Order {OrderId} not found for session {SessionId}", orderId, sessionIdStr);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
    }
  }

  /// <summary>
  /// Processes recipe credit purchases.
  /// </summary>
  private async Task ProcessCreditPurchase(string customerEmail, int purchasedQuantity)
  {
    logger.LogInformation("Processing recipe credit purchase for customer {Email}, quantity {Quantity}",
      customerEmail, purchasedQuantity);

    try
    {
      var customer = await customerService.GetOrCreateCustomer(customerEmail);
      await customerService.AddRecipes(customer, purchasedQuantity);

      logger.LogInformation("Credit purchase successful. Added {Quantity} recipes to customer {Email}.",
        purchasedQuantity, customer.Email);

      await ProcessPendingOrders(customer);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing credit purchase for customer {Email}", customerEmail);
    }
  }

  /// <summary>
  /// Processes any pending orders for the customer.
  /// </summary>
  private async Task ProcessPendingOrders(Customer customer)
  {
    try
    {
      var pendingOrders = await orderRepository.GetPendingOrdersForCustomer(customer.Email);

      if (pendingOrders.Count == 0)
      {
        logger.LogInformation("No pending orders found for customer {Email}", customer.Email);
        return;
      }

      logger.LogInformation("Found {Count} pending orders for customer {Email}",
        pendingOrders.Count, customer.Email);

      foreach (var order in pendingOrders)
      {
        if (customer.AvailableRecipes > 0)
        {
          orderService.QueueOrderProcessing(order.OrderId);
          logger.LogInformation("Queued order {OrderId} for processing", order.OrderId);
        }
        else
        {
          logger.LogInformation("Customer {Email} has insufficient credits for order {OrderId}",
            customer.Email, order.OrderId);
          break;
        }
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing pending orders for customer {Email}", customer.Email);
    }
  }

  /// <summary>
  /// Handles payment intent succeeded events.
  /// </summary>
  private async Task HandlePaymentIntentSucceeded(dynamic stripeEvent)
  {
    try
    {
      var paymentIntent = stripeEvent.Data.Object;
      var paymentIntentId = paymentIntent.Id;

      logger.LogInformation("Payment intent succeeded: {PaymentIntentId}", (string)paymentIntentId);

      if (paymentIntent.Metadata != null)
      {
        var metadataOrderId = paymentIntent.Metadata.order_id as string;
        if (!string.IsNullOrEmpty(metadataOrderId))
        {
          logger.LogInformation("Found order_id in payment intent metadata: {OrderId}", metadataOrderId);
          var mockEvent = new { Data = new { Object = CreateMockSession(metadataOrderId) } };
          await HandleCheckoutSessionCompleted(mockEvent);
          return;
        }
      }

      var sessions = await stripeService.FindSessionsByPaymentIntent(paymentIntentId);
      if (sessions.Any())
      {
        var session = sessions.First();
        logger.LogInformation("Found session {SessionId} for payment intent {PaymentIntentId}",
          (string)session.Id, (string)paymentIntentId);

        var mockEvent = new { Data = new { Object = session } };
        await HandleCheckoutSessionCompleted(mockEvent);
      }
      else
      {
        logger.LogWarning("No session found for payment intent {PaymentIntentId}", (string)paymentIntentId);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing payment intent");
    }
  }

  /// <summary>
  /// Handles charge succeeded events.
  /// </summary>
  private async Task HandleChargeSucceeded(dynamic stripeEvent)
  {
    try
    {
      var charge = stripeEvent.Data.Object;
      var chargeId = charge.Id;
      var paymentIntentId = charge.PaymentIntentId;

      logger.LogInformation("Charge succeeded: {ChargeId} for payment intent {PaymentIntentId}",
        (object)chargeId, (string)paymentIntentId);

      if (!string.IsNullOrEmpty(paymentIntentId))
      {
        var paymentIntent = await stripeService.GetPaymentIntent(paymentIntentId);
        var mockEvent = new { Data = new { Object = paymentIntent } };
        await HandlePaymentIntentSucceeded(mockEvent);
      }
      else if (charge.Metadata != null)
      {
        var orderId = charge.Metadata.order_id as string;
        if (!string.IsNullOrEmpty(orderId))
        {
          logger.LogInformation("Found order_id in charge metadata: {OrderId}", orderId);
          var mockEvent = new { Data = new { Object = CreateMockSession(orderId) } };
          await HandleCheckoutSessionCompleted(mockEvent);
        }
        else
        {
          logger.LogWarning("Charge {ChargeId} has no payment intent ID or order_id in metadata", (string)chargeId);
        }
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing charge");
    }
  }

  /// <summary>
  /// Handles async payment failed events.
  /// </summary>
  private async Task HandleAsyncPaymentFailed(dynamic stripeEvent)
  {
    try
    {
      var session = stripeEvent.Data.Object;
      var orderId = session.ClientReferenceId as string;

      if (string.IsNullOrEmpty(orderId))
      {
        logger.LogWarning("Received checkout.session.async_payment_failed event without client_reference_id");
        return;
      }

      var order = await orderService.GetOrder(orderId);
      order.Status = OrderStatus.Failed;

      var sessionIdStr = session.Id as string ?? "unknown";
      logger.LogInformation("Payment failed for order {OrderId}, session {SessionId}", orderId, sessionIdStr);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing failed payment");
    }
  }

  /// <summary>
  /// Creates a mock session object for event processing.
  /// </summary>
  private dynamic CreateMockSession(string orderId) =>
    new { Id = "mock_" + Guid.NewGuid(), ClientReferenceId = orderId };
}
