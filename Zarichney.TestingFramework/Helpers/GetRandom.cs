using AutoFixture;

namespace Zarichney.TestingFramework.Helpers;

public static class GetRandom
{
    private static readonly Fixture _fixture = new();

    public static T Create<T>() => _fixture.Create<T>();
    
    public static string String() => _fixture.Create<string>();
    
    public static int Int() => _fixture.Create<int>();
    
    public static bool Bool() => _fixture.Create<bool>();
    
    public static DateTime DateTime() => _fixture.Create<DateTime>();
}