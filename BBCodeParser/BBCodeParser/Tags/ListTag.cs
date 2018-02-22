namespace BBCodeParser.Tags
{
    public class ListTag : Tag
    {
        public ListTag(string name, string openTag, string closeTag, bool withAttribute = false, bool secure = true)
            : base(name, openTag, closeTag, withAttribute, secure)
        {
        }

        public ListTag(string name, string openTag, bool withAttribute = false, bool secure = true)
            : base(name, openTag, withAttribute, secure)
        {
        }
    }
}