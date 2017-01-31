﻿using System.Linq;
using System.Text.RegularExpressions;

namespace BBCodeParser.Tags
{
    public class Tag
    {
	    private static readonly Regex JsXssSecureRegex = new Regex("(javascript|data):", RegexOptions.IgnoreCase);

	    private static readonly Regex[] EscapeRegexes =
	    {
		    new Regex("\"|'|`|\\n|\\s|\\t|\\r", RegexOptions.IgnoreCase),
			new Regex("&#[\\d\\w]+;?", RegexOptions.IgnoreCase)
	    };

        private string OpenTag { get; set; }
        private string CloseTag { get; set; }
        public bool WithAttribute { get; private set; }
        public bool RequiresClosing { get; private set; }
        private bool Secure { get; set; }
        public string Name { get; private set; }

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

		    var result = EscapeRegexes.Aggregate(attributeValue, (input, regex) => regex.Replace(input, string.Empty));
	        return JsXssSecureRegex.Replace(result, "_xss_");
        }
    }
}