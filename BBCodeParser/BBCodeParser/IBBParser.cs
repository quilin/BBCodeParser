using BBCodeParser.Nodes;

namespace BBCodeParser
{
	public interface IBbParser
	{
		NodeTree Parse(string input);
	}
}