namespace Domain.App.Core.Utility;

public static class StringExtensions
{
    public static string IfNullOrWhiteSpaceFallbackTo(this string source, string fallback) =>
        string.IsNullOrWhiteSpace(source) ? fallback : source;
}