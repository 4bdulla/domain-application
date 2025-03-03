namespace Domain.App.Core.Utility;

public static class StringExtensions
{
    public static string FallbackTo(this string source, string fallback) => string.IsNullOrWhiteSpace(source) ? fallback : source;
}