using Zarichney.Services.Status;
using Zarichney.Config;

namespace Zarichney.Services.Payment;

public enum PaymentType
{
  OrderCompletion,
  RecipeCredit
}

public class PaymentConfig : IConfig
{
  [RequiresConfiguration(ExternalServices.Payments)]
  public string StripeSecretKey { get; init; } = string.Empty;

  [RequiresConfiguration(ExternalServices.Payments)]
  public string StripePublishableKey { get; init; } = string.Empty;

  [RequiresConfiguration(ExternalServices.Payments)]
  public string StripeWebhookSecret { get; init; } = string.Empty;

  public string SuccessUrl { get; init; } = "/order/success/{0}";
  public string CancelUrl { get; init; } = "/order/cancel/{0}";
  public decimal RecipePrice { get; init; } = 1.00m;
  public string Currency { get; init; } = "usd";
  public int[] RecipePackageSizes { get; init; } = [5, 10, 20, 50];
}

/// <summary>
/// Information about a checkout session that can be shared with clients
/// </summary>
public class CheckoutSessionInfo
{
  public string Id { get; set; } = string.Empty;
  public string Status { get; set; } = string.Empty;
  public string? CustomerEmail { get; set; }
  public decimal? AmountTotal { get; set; }
  public string? Currency { get; set; }
  public string? PaymentStatus { get; set; }
}

public class StripeSessionMetadata
{
  public PaymentType PaymentType { get; set; } = PaymentType.OrderCompletion;
  public string CustomerEmail { get; set; } = string.Empty;
  public int? RecipeCount { get; set; }
}
