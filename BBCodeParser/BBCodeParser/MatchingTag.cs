using System.Text.RegularExpressions;

namespace BBCodeParser
{
    internal class MatchingTag
    {
        public MatchingTag()
        {
            TagType = TagType.NoResult;
        }

        public Match Match { get; set; }
        public BBTag Tag { get; set; }
        public TagType TagType { get; set; }
    }
}