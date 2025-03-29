namespace Zarichney.Server.Config;

public class NotExpectedException(string s) : Exception($"Internal server error. Not expected: {s}");