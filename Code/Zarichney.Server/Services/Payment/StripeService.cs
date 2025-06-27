using Stripe;
using Stripe.Checkout;
using Zarichney.Services.Status;

namespace Zarichney.Services.Payment;

/// <summary>
/// Interface for Stripe-specific operations
/// </summary>
public interface IStripeService
{
  /// <summary>
  /// Creates a Stripe checkout session and returns the session ID
  /// </summary>
  Task<string> CreateCheckoutSession(
    string customerEmail,
    int quantity,
    Dictionary<string, string> metadata,
    string successUrl,
    string cancelUrl,
    string productName,
    string productDescription,
    string? clientReferenceId = null);

  /// <summary>
  /// Creates a Stripe checkout session and returns the full session including the checkout URL
  /// </summary>
  Task<Session> CreateCheckoutSessionWithUrl(
    string customerEmail,
    int quantity,
    Dictionary<string, string> metadata,
    string successUrl,
    string cancelUrl,
    string productName,
    string productDescription,
    string? clientReferenceId = null);

  /// <summary>
  /// Gets a session by ID
  /// </summary>
  Task<Session> GetSession(string sessionId);

  /// <summary>
  /// Gets a session with expanded line items
  /// </summary>
  Task<Session> GetSessionWithLineItems(string sessionId);

  /// <summary>
  /// Validates and constructs a Stripe event from webhook data
  /// </summary>
  Event ConstructEvent(string requestBody, string signature, string webhookSecret);

  /// <summary>
  /// Finds sessions associated with a payment intent
  /// </summary>
  Task<List<Session>> FindSessionsByPaymentIntent(string paymentIntentId);

  /// <summary>
  /// Gets a payment intent by ID
  /// </summary>
  Task<PaymentIntent> GetPaymentIntent(string paymentIntentId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a mock Stripe event for testing or handler reuse
  /// </summary>
  Event CreateMockEvent<T>(T obj) where T : IStripeEntity;

  StripeSessionMetadata ParseSessionMetadata(Session sessionWithLineItems);
  int GetPurchasedQuantity(Session sessionWithLineItems);

  /// <summary>
  /// Creates a payment intent.
  /// </summary>
  Task<object> CreatePaymentIntentAsync(
    string customerEmail,
    long amount,
    string currency,
    Dictionary<string, string> metadata,
    CancellationToken cancellationToken = default);
}

/// <summary>
/// Service that encapsulates all direct interactions with Stripe API
/// </summary>
public class StripeService : IStripeService
{
  private readonly PaymentConfig _config;
  private readonly ILogger<StripeService> _logger;

  public StripeService(PaymentConfig config, ILogger<StripeService> logger)
  {
    _config = config;
    _logger = logger;

    // Log a warning if the key is missing at startup, but don't set it globally here.
    if (string.IsNullOrEmpty(_config.StripeSecretKey))
    {
      _logger.LogWarning("Stripe Secret Key is missing in configuration. Stripe functionality will be unavailable until configured.");
    }
  }

  /// <summary>
  /// Ensures the Stripe Secret Key is configured.
  /// </summary>
  private void EnsureStripeKeyConfigured()
  {
    // ServiceUnavailableException will be thrown by the proxy if Payment service is unavailable
    if (string.IsNullOrEmpty(_config.StripeSecretKey))
    {
      _logger.LogError("Stripe operation failed: Stripe Secret Key is missing");
      throw new ServiceUnavailableException("Payment service is unavailable due to missing Stripe Secret Key configuration");
    }
    StripeConfiguration.ApiKey = _config.StripeSecretKey;
  }

  /// <summary>
  /// Ensures the Stripe Webhook Secret is configured.
  /// </summary>
  private void EnsureWebhookSecretConfigured()
  {
    // ServiceUnavailableException will be thrown by the proxy if Payment service is unavailable
    if (string.IsNullOrEmpty(_config.StripeWebhookSecret))
    {
      _logger.LogError("Stripe operation failed: Stripe Webhook Secret is missing");
      throw new ServiceUnavailableException("Payment service is unavailable due to missing Stripe Webhook Secret configuration");
    }
  }

  /// <summary>
  /// Creates a Stripe checkout session with the specified parameters and returns session ID
  /// </summary>
  public async Task<string> CreateCheckoutSession(
    string customerEmail,
    int quantity,
    Dictionary<string, string> metadata,
    string successUrl,
    string cancelUrl,
    string productName,
    string productDescription,
    string? clientReferenceId = null)
  {
    EnsureStripeKeyConfigured();
    var session = await CreateCheckoutSessionInternal(
      customerEmail,
      quantity,
      metadata,
      successUrl,
      cancelUrl,
      productName,
      productDescription,
      clientReferenceId);

    return session.Id;
  }

  /// <summary>
  /// Creates a Stripe checkout session with the specified parameters and returns the full session including URL
  /// </summary>
  public async Task<Session> CreateCheckoutSessionWithUrl(
    string customerEmail,
    int quantity,
    Dictionary<string, string> metadata,
    string successUrl,
    string cancelUrl,
    string productName,
    string productDescription,
    string? clientReferenceId = null)
  {
    EnsureStripeKeyConfigured();
    return await CreateCheckoutSessionInternal(
      customerEmail,
      quantity,
      metadata,
      successUrl,
      cancelUrl,
      productName,
      productDescription,
      clientReferenceId);
  }

  /// <summary>
  /// Internal helper method to create a checkout session
  /// </summary>
  private async Task<Session> CreateCheckoutSessionInternal(
    string customerEmail,
    int quantity,
    Dictionary<string, string> metadata,
    string successUrl,
    string cancelUrl,
    string productName,
    string productDescription,
    string? clientReferenceId = null)
  {
    var sessionService = new SessionService();
    var session = await sessionService.CreateAsync(new SessionCreateOptions
    {
      CustomerEmail = customerEmail,
      PaymentMethodTypes = ["card"],
      LineItems = [
        new SessionLineItemOptions
        {
          PriceData = new SessionLineItemPriceDataOptions
          {
            Currency = _config.Currency,
            UnitAmount = (long)(_config.RecipePrice * 100), // Convert to cents
            ProductData = new SessionLineItemPriceDataProductDataOptions
            {
              Name = productName,
              Description = productDescription
            }
          },
          Quantity = quantity
        }
      ],
      Mode = "payment",
      SuccessUrl = successUrl,
      CancelUrl = cancelUrl,
      Metadata = metadata,
      ClientReferenceId = clientReferenceId
    });

    _logger.LogInformation(
      "Created Stripe checkout session {SessionId} for customer {Email} with quantity {Quantity}",
      session.Id, customerEmail, quantity);

    return session;
  }

  /// <summary>
  /// Gets a session by ID
  /// </summary>
  public async Task<Session> GetSession(string sessionId)
  {
    EnsureStripeKeyConfigured();
    var service = new SessionService();
    return await service.GetAsync(sessionId);
  }

  /// <summary>
  /// Gets a session with expanded line items and metadata
  /// </summary>
  public async Task<Session> GetSessionWithLineItems(string sessionId)
  {
    EnsureStripeKeyConfigured();
    var service = new SessionService();
    return await service.GetAsync(sessionId, new SessionGetOptions
    {
      Expand = ["line_items", "metadata"]
    });
  }

  /// <summary>
  /// Validates and constructs a Stripe event from webhook data
  /// </summary>
  public Event ConstructEvent(string requestBody, string signature, string webhookSecret)
  {
    EnsureWebhookSecretConfigured();
    return EventUtility.ConstructEvent(requestBody, signature, webhookSecret);
  }

  /// <summary>
  /// Finds sessions associated with a payment intent
  /// </summary>
  public async Task<List<Session>> FindSessionsByPaymentIntent(string paymentIntentId)
  {
    EnsureStripeKeyConfigured();
    var sessionService = new SessionService();
    var sessions = await sessionService.ListAsync(new SessionListOptions
    {
      PaymentIntent = paymentIntentId,
      Limit = 1
    });

    return sessions.Data.ToList();
  }

  /// <summary>
  /// Gets a payment intent by ID
  /// </summary>
  public async Task<PaymentIntent> GetPaymentIntent(string paymentIntentId, CancellationToken cancellationToken = default)
  {
    EnsureStripeKeyConfigured();
    var paymentIntentService = new PaymentIntentService();
    return await paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);
  }

  /// <summary>
  /// Creates a mock Stripe event for testing or handler reuse
  /// </summary>
  public Event CreateMockEvent<T>(T obj) where T : IStripeEntity
  {
    return new Event
    {
      Data = new EventData
      {
        Object = (IHasObject)obj
      }
    };
  }

  /// <summary>
  /// Parses a Stripe session's metadata into a strongly typed model.
  /// </summary>
  public StripeSessionMetadata ParseSessionMetadata(Session session)
  {
    var metadata = session.Metadata;
    var result = new StripeSessionMetadata();

    if (metadata != null)
    {
      if (metadata.TryGetValue("payment_type", out var paymentTypeStr) &&
          Enum.TryParse(paymentTypeStr, out PaymentType parsedType))
      {
        result.PaymentType = parsedType;
      }

      if (metadata.TryGetValue("customer_email", out var email))
      {
        result.CustomerEmail = email;
      }

      if (metadata.TryGetValue("recipe_count", out var recipeCountStr) &&
          int.TryParse(recipeCountStr, out var count))
      {
        result.RecipeCount = count;
      }
    }

    // Fallback to session property if metadata is missing
    if (string.IsNullOrEmpty(result.CustomerEmail) && !string.IsNullOrEmpty(session.CustomerEmail))
    {
      result.CustomerEmail = session.CustomerEmail;
    }

    return result;
  }

  /// <summary>
  /// Calculates the total purchased quantity from the session's line items.
  /// </summary>
  public int GetPurchasedQuantity(Session session)
  {
    var purchasedQuantity = 0;
    if (session.LineItems?.Data != null)
    {
      purchasedQuantity = session.LineItems.Data.Aggregate(purchasedQuantity, (current, item) => (int)(current + item.Quantity.GetValueOrDefault()));
    }
    return purchasedQuantity;
  }

  /// <summary>
  /// Creates a payment intent using Stripe.
  /// </summary>
  public async Task<object> CreatePaymentIntentAsync(
    string customerEmail,
    long amount,
    string currency,
    Dictionary<string, string> metadata,
    CancellationToken cancellationToken = default)
  {
    EnsureStripeKeyConfigured();
    var paymentIntentService = new PaymentIntentService();
    var options = new PaymentIntentCreateOptions
    {
      Amount = amount,
      Currency = currency,
      Metadata = metadata
    };
    var intent = await paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);
    return intent;
  }
}
