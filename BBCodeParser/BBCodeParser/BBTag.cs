using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BBCodeParser
{
    public class BBTag
    {
        public BBTag(string bbTag, string openingHtmlTag, string closingHtmlTag,
                     Dictionary<string, string> attributes = null, bool selfAttributed = false)
        {
            BbTag = bbTag;
            OpeningHtmlTag = openingHtmlTag;
            ClosingHtmlTag = closingHtmlTag;
            RequiresClosingTag = true;
            Attributes = attributes;
            SelfAttributed = selfAttributed;
        }

        public BBTag(string bbTag, string openingHtmlTag,
                     Dictionary<string, string> attributes = null, bool selfAttributed = false)
        {
            BbTag = bbTag;
            OpeningHtmlTag = openingHtmlTag;
            RequiresClosingTag = false;
            Attributes = attributes;
            SelfAttributed = selfAttributed;
        }

        internal string BbTag { get; set; }
        internal bool RequiresClosingTag { get; set; }

        private Dictionary<string, string> Attributes { get; set; }
        private bool SelfAttributed { get; set; }
        private string OpeningHtmlTag { get; set; }
        private string ClosingHtmlTag { get; set; }

        internal string GetOpenBbTagPattern(DirectionMode directionMode)
        {
            var attributeFormatPattern = directionMode == DirectionMode.BBToHtml
                                             ? "(?<{0}>.*?)"
                                             : "${{{0}}}";

            return string.Format(@"{0}[{1}{2}]",
                directionMode == DirectionMode.BBToHtml ? @"\" : string.Empty,
                SelfAttributed
                    ? string.Format("{0}=\"{1}\"",
                        BbTag,
                        string.Format(attributeFormatPattern, BbTag))
                    : BbTag,
                Attributes == null || Attributes.Count == 0
                    ? string.Empty
                    : string.Format(" {0}", 
                        string.Join(" ", Attributes.Select(k => string.Format("{0}=\"{1}\"",
                            k.Key,
                            string.Format(attributeFormatPattern, k.Key))))));
        }

        internal string GetCloseBbTagPattern(DirectionMode directionMode)
        {
            return directionMode == DirectionMode.BBToHtml
                       ? string.Format(@"\[\/{0}]", BbTag)
                       : string.Format("[/{0}]", BbTag);
        }

        internal string GetOpenHtmlTagPattern(DirectionMode directionMode)
        {
            var attributeFormatPattern = directionMode == DirectionMode.HtmlToBB
                                             ? "(?<{0}>.*?)"
                                             : "${{{0}}}";

            if ((Attributes == null || Attributes.Count == 0) && !SelfAttributed)
                return OpeningHtmlTag;

            var result = OpeningHtmlTag;

            if (SelfAttributed)
            {
                var regex = new Regex(string.Format(@"\{{{0}}}", BbTag));
                result = regex.Replace(result, string.Format(attributeFormatPattern, BbTag));
            }

            if (Attributes != null && Attributes.Count > 0)
            {
                foreach (var attribute in Attributes)
                {
                    var regex = new Regex(string.Format(@"\{{{0}}}", attribute.Key));
                    result = regex.Replace(result, string.Format(attributeFormatPattern, attribute.Key));
                }
            }

            return result;
        }

        internal string GetCloseHtmlTagPattern()
        {
            return ClosingHtmlTag;
        }
    }
}