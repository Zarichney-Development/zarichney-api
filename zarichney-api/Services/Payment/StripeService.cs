using Stripe;
using Stripe.Checkout;

namespace Zarichney.Services.Payment;

/// <summary>
/// Interface for Stripe-specific operations
/// </summary>
public interface IStripeService
{
  /// <summary>
  /// Creates a Stripe checkout session
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
  Task<PaymentIntent> GetPaymentIntent(string paymentIntentId);

  /// <summary>
  /// Creates a mock Stripe event for testing or handler reuse
  /// </summary>
  Event CreateMockEvent<T>(T obj) where T : IStripeEntity;

  StripeSessionMetadata ParseSessionMetadata(Session sessionWithLineItems);
  int GetPurchasedQuantity(Session sessionWithLineItems);
}

/// <summary>
/// Service that encapsulates all direct interactions with Stripe API
/// </summary>
public class StripeService : IStripeService
{
  private readonly PaymentConfig _config;

  public StripeService(PaymentConfig config)
  {
    _config = config;

    // Initialize Stripe API
    StripeConfiguration.ApiKey = _config.StripeSecretKey;
  }

  /// <summary>
  /// Creates a Stripe checkout session with the specified parameters
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
    var options = new SessionCreateOptions
    {
      PaymentMethodTypes = ["card"],
      LineItems =
      [
        new SessionLineItemOptions
        {
          PriceData = new SessionLineItemPriceDataOptions
          {
            UnitAmount = (long)(Math.Round(_config.RecipePrice * 100)), // Stripe uses cents
            Currency = _config.Currency,
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
      CustomerEmail = customerEmail,
      Metadata = metadata
    };

    // Only set ClientReferenceId if provided (needed for orders, not for credit purchases)
    if (!string.IsNullOrEmpty(clientReferenceId))
    {
      options.ClientReferenceId = clientReferenceId;
    }

    var service = new SessionService();
    var session = await service.CreateAsync(options);

    return session.Id;
  }

  /// <summary>
  /// Gets a session by ID
  /// </summary>
  public async Task<Session> GetSession(string sessionId)
  {
    var service = new SessionService();
    return await service.GetAsync(sessionId);
  }

  /// <summary>
  /// Gets a session with expanded line items and metadata
  /// </summary>
  public async Task<Session> GetSessionWithLineItems(string sessionId)
  {
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
    return EventUtility.ConstructEvent(requestBody, signature, webhookSecret);
  }

  /// <summary>
  /// Finds sessions associated with a payment intent
  /// </summary>
  public async Task<List<Session>> FindSessionsByPaymentIntent(string paymentIntentId)
  {
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
  public async Task<PaymentIntent> GetPaymentIntent(string paymentIntentId)
  {
    var paymentIntentService = new PaymentIntentService();
    return await paymentIntentService.GetAsync(paymentIntentId);
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
  /// Maps a Stripe Session to a CheckoutSessionInfo domain model.
  /// </summary>
  public CheckoutSessionInfo MapToCheckoutSessionInfo(Session session)
  {
    return new CheckoutSessionInfo
    {
      Id = session.Id,
      Status = session.Status,
      CustomerEmail = session.CustomerEmail,
      AmountTotal = session.AmountTotal / 100m,
      Currency = session.Currency,
      PaymentStatus = session.PaymentStatus
    };
  }

  /// <summary>
  /// Creates a mock Stripe session for order completion.
  /// </summary>
  public Session CreateMockSessionForOrder(string orderId)
  {
    // This assumes you want a simple Session-like object. Note: 
    // Stripe.Session is normally provided by the Stripe API,
    // so this is a simplified example for internal use.
    return new Session
    {
      Id = "mock_" + Guid.NewGuid(),
      ClientReferenceId = orderId
    };
  }
}