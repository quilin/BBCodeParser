using System.Linq;
using System.Text.RegularExpressions;

namespace BBCodeParser.Tags
{
    public class Tag
    {
        private static readonly Regex JsXssSecureRegex = new Regex("(javascript|data):", RegexOptions.IgnoreCase);

        private static readonly Regex[] EscapeRegexes =
        {
            new Regex("\"|'|`|\\n|\\s|\\t|\\r|\\<|\\>", RegexOptions.IgnoreCase),
            new Regex("&#[\\d\\w]+;?", RegexOptions.IgnoreCase)
        };

        private string OpenTag { get; }
        private string CloseTag { get; }
        public bool WithAttribute { get; }
        public bool RequiresClosing { get; }
        private bool Secure { get; }
        public string Name { get; }

        public Tag(string name, string openTag, string closeTag, bool withAttribute = false, bool secure = true)
        {
            OpenTag = openTag;
            CloseTag = closeTag;
            WithAttribute = withAttribute;
            Secure = secure;
            RequiresClosing = true;
            Name = name;
        }

        public Tag(string name, string openTag, bool withAttribute = false, bool secure = true)
        {
            OpenTag = openTag;
            CloseTag = null;
            WithAttribute = withAttribute;
            Secure = secure;
            RequiresClosing = false;
            Name = name;
        }

        public string GetOpenHtml(string attributeValue)
        {
            return GetHtmlPart(OpenTag, attributeValue);
        }

        public string GetCloseHtml(string attributeValue)
        {
            return GetHtmlPart(CloseTag, attributeValue);
        }

        private string GetHtmlPart(string tagPart, string attributeValue)
        {
            return WithAttribute ? tagPart.Replace("{value}", GetAttributeValueForHtml(attributeValue)) : tagPart;
        }

        private string GetAttributeValueForHtml(string attributeValue)
        {
            if (!Secure)
            {
                return attributeValue;
            }

            return Secure
                ? JsXssSecureRegex.Replace(EscapeSpecialCharacters(attributeValue), "_xss_")
                : attributeValue;
        }

        private static string EscapeSpecialCharacters(string value)
        {
            while (true)
            {
                var escaped = EscapeRegexes.Aggregate(value, (input, regex) => regex.Replace(input, string.Empty));
                if (escaped == value)
                {
                    return escaped;
                }
                value = escaped;
            }
        }
    }
}