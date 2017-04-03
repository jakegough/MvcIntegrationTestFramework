using System;

namespace MvcIntegrationTestFramework.Browsing
{
    public static class MvcUtils
    {
        public static string ExtractAntiForgeryToken(string htmlResponseText)
        {
            if (htmlResponseText == null) throw new ArgumentNullException("htmlResponseText");

            const string pattern = "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
            var idx1 = htmlResponseText.IndexOf(pattern, StringComparison.Ordinal);
            if (idx1 < 1) return null;
            idx1 += pattern.Length;
            var idx2 = htmlResponseText.IndexOf("\"", idx1, StringComparison.Ordinal);
            return htmlResponseText.Substring(idx1, idx2 - idx1);
        }
    }
}