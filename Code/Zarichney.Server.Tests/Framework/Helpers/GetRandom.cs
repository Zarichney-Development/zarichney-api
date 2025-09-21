using AutoFixture;

namespace Zarichney.Server.Tests.Framework.Helpers;

public static class GetRandom
{
  private static readonly Fixture Fixture = new();

  public static string String(int? length = null)
  {
    var str = Fixture.Create<string>();
    return length.HasValue ? str[..Math.Min(length.Value, str.Length)] : str;
  }

  public static int Int(int min = int.MinValue, int max = int.MaxValue) =>
      Fixture.Create<int>() % (max - min) + min;

  public static decimal Decimal(decimal min = decimal.MinValue, decimal max = decimal.MaxValue) =>
      (decimal)Fixture.Create<double>() % (max - min) + min;

  public static bool Bool() => Fixture.Create<bool>();

  public static DateTime DateTime(DateTime? min = null, DateTime? max = null)
  {
    var date = Fixture.Create<DateTime>();
    if (min.HasValue && date < min.Value) date = min.Value;
    if (max.HasValue && date > max.Value) date = max.Value;
    return date;
  }

  public static Uri Uri() => new(String());

  public static string Email() => $"{String(10)}@{String(8)}.com";

  public static T Enum<T>() where T : struct => Fixture.Create<T>();

  public static string Password() => String(12) + "A1!"; // Ensures minimum complexity

  public static Guid Guid() => Fixture.Create<Guid>();
}
