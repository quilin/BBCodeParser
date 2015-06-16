using System.Collections.Generic;
using System.Text;

namespace BBCodeParser
{
    public class BBParser : IBBParser
    {
        private readonly IEnumerable<BBTag> tags;
        private readonly IDictionary<string, string> replacements;
        private Reader reader;
        private readonly Stack<TagResult> stack;

        public BBParser(IEnumerable<BBTag> tags, IDictionary<string, string> replacements = null)
        {
            this.tags = tags;
            this.replacements = replacements;
            stack = new Stack<TagResult>();
        }

        public string Parse(string input, DirectionMode directionMode)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
                
            if (replacements != null)
                foreach (var replacement in replacements)
                    input = input.Replace(
                        directionMode == DirectionMode.BBToHtml ? replacement.Key : replacement.Value,
                        directionMode == DirectionMode.BBToHtml ? replacement.Value : replacement.Key);

            return directionMode == DirectionMode.HtmlToBB ? ParseToBBCode(input) : ParseToHtml(input);
        }
        
        private string ParseToHtml(string input)
        {
            return Parse(input, new BBToHtmlReadingStrategy(tags));
        }

        private string ParseToBBCode(string input)
        {
            return Parse(input, new HtmlToBBReadingStrategy(tags));
        }
        
        private string Parse(string input, IReadingStrategy readingStrategy)
        {
            reader = new Reader(readingStrategy);

            var result = new StringBuilder();
            while (!string.IsNullOrEmpty(input))
            {
                var tagResult = reader.Read(input);
                result.Append(tagResult.Text);
                if (tagResult.TagType == TagType.Open)
                {
                    if (tagResult.Tag.RequiresClosingTag)
                    {
                        stack.Push(tagResult);
                    }
                    result.Append(tagResult.OpeningTagValue);
                }
                else if (tagResult.TagType == TagType.Close)
                {
                    if (stack.Count > 0)
                    {
                        TagResult tagToBeClosed;
                        do
                        {
                            tagToBeClosed = stack.Pop();
                            result.Append(tagToBeClosed.ClosingTagValue);
                        } while (stack.Count > 0 && tagToBeClosed.Tag.BbTag != tagResult.Tag.BbTag);
                    }
                }
                input = tagResult.RemainingInput;
            }

            while (stack.Count > 0)
                result.Append(stack.Pop().ClosingTagValue);

            return result.ToString();
        }
    }
}
