namespace BBCodeParser.Tags
{
    public class PreformattedTag : Tag
    {
        public PreformattedTag(string name, string openTag, string closeTag, bool withAttribute = false, bool secure = true) : base(name, openTag, closeTag, false, false)
        {
        }

        public PreformattedTag(string name, string openTag, bool withAttribute = false, bool secure = true) : base(name, openTag, false, false)
        {
        }
    }
}