using System.Linq;
using System.Text.RegularExpressions;
using BBCodeParser.Tags;

namespace BBCodeParser
{
    public class Reader
    {
        private readonly string input;
        private readonly Tag[] tags;
        private int position;
        private Match match;
        private readonly Regex bbPattern = new Regex(@"\[(?<closing>\/)?(?<tag>\w+)(\=\""(?<value>.*?)\"")?\]");

        public Reader(string input, Tag[] tags)
        {
            this.input = input;
            this.tags = tags;
            position = 0;
            match = bbPattern.Match(this.input);
        }

        public bool TryRead(out TagResult result)
        {
            if (position == input.Length)
            {
                result = null;
                return false;
            }

            result = new TagResult();
            if (match.Success)
            {
                var tagName = match.Groups["tag"].Value;
                var matchingTag = tags.FirstOrDefault(t => t.Name == tagName);

                if (matchingTag == null)
                {
                    result.Text = input.Substring(position, match.Index + match.Length - position);
                    result.TagType = TagType.NoResult;
                }
                else
                {
                    result.Match = match.Value;
                    result.Text = input.Substring(position, match.Index - position);
                    result.Tag = matchingTag;
                    result.AttributeValue = match.Groups["value"].Value;
                    result.TagType = match.Groups["closing"].Success ? TagType.Close : TagType.Open;
                }

                position = match.Index + match.Length;
                match = match.NextMatch();
                return true;
            }

            result.Text = input.Substring(position);
            position = input.Length;
            result.TagType = TagType.NoResult;
            return true;
        }
    }
}