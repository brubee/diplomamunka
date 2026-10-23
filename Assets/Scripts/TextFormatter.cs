using System.Text.RegularExpressions;

public static class TextFormatter
{
    public static string HighlightLetters(string text)
    {
        return Regex.Replace(text, "jj|j|lly|ly", match => { return $"<color=red>{match.Value}</color>"; }, RegexOptions.IgnoreCase);
    }
}
