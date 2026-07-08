using System.Text.RegularExpressions;

public static class RemoveHtmlTagHelper
{
    public static string RemoveHtmlTags(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

       
        return Regex.Replace(input, "<.*?>", string.Empty);
    }
}
