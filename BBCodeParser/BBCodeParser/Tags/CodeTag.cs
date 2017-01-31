namespace BBCodeParser.Tags
{
	public class CodeTag : Tag
	{
		public CodeTag(string name, string openTag, string closeTag, bool withAttribute = false, bool secure = true) : base(name, openTag, closeTag, false, false)
		{
		}

		public CodeTag(string name, string openTag, bool withAttribute = false, bool secure = true) : base(name, openTag, false, false)
		{
		}
	}
}