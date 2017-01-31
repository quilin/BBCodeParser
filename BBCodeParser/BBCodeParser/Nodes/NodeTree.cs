using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBCodeParser.Nodes
{
	public class NodeTree : Node
	{
	    private readonly Dictionary<string, string> securitySubstitutions;
	    private readonly Dictionary<string, string> aliasSubstitutions;

	    public NodeTree(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions
            )
		{
		    this.securitySubstitutions = securitySubstitutions;
		    this.aliasSubstitutions = aliasSubstitutions;
		    ChildNodes = new List<Node>();
			ParentNode = null;
		}

		public string ToHtml(Func<Node, bool> filter = null, Func<Node, string, string> filterAttributeValue = null)
		{
			return ToHtml(securitySubstitutions, aliasSubstitutions, filter, filterAttributeValue);
		}

	    public string ToText()
	    {
	        return ToText(securitySubstitutions, aliasSubstitutions);
	    }

        internal override string ToHtml(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions,
            Func<Node, bool> filter = null,
            Func<Node, string, string> filterAttributeValue = null)
        {
			var result = new StringBuilder(ChildNodes.Count);
			foreach (var childNode in ChildNodes.Where(n => filter == null || filter(n)))
			{
				result.Append(childNode.ToHtml(securitySubstitutions, aliasSubstitutions, filter, filterAttributeValue));
			}
			return result.ToString();
		}

	    internal override string ToText(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions
            )
        {
			var result = new StringBuilder(ChildNodes.Count);
            foreach (var childNode in ChildNodes)
            {
                result.Append(childNode.ToText(securitySubstitutions, aliasSubstitutions));
            }
            return result.ToString();
	    }

	    public override string ToBb(Dictionary<string, string> substitutions = null)
		{
			var result = new StringBuilder(ChildNodes.Count);
			foreach (var childNode in ChildNodes)
			{
				result.Append(childNode.ToBb());
			}
			return result.ToString();
		}
	}
}