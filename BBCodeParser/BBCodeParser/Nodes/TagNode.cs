using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBCodeParser.Tags;

namespace BBCodeParser.Nodes
{
	public class TagNode : Node
	{
		public string AttributeValue { get; }
		public Tag Tag { get; }

		public TagNode(Tag tag, Node parent, string attributeValue)
		{
			Tag = tag;
			AttributeValue = attributeValue;
			ChildNodes = new List<Node>();
			ParentNode = parent;
		}

        internal override string ToHtml(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions,
            Func<Node, bool> filter = null,
            Func<Node, string, string> filterAttributeValue = null)
        {
		    var attributeValue = filterAttributeValue == null ? AttributeValue : filterAttributeValue(this, AttributeValue);
			var result = new StringBuilder(Tag.GetOpenHtml(attributeValue), ChildNodes.Count + 2);
			foreach (var childNode in ChildNodes.Where(n => filter == null || filter(n)))
			{
				if (Tag is CodeTag)
				{
					result.Append(childNode.ToBb(securitySubstitutions));
				}
                else if (Tag is PreformattedTag)
				{
				    result.Append(childNode.ToHtml(securitySubstitutions, null, filter));
				}
				else
				{
					result.Append(childNode.ToHtml(securitySubstitutions, aliasSubstitutions, filter));
				}
			}
			if (Tag.RequiresClosing)
			{
				result.Append(Tag.GetCloseHtml(attributeValue));
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
				if (Tag is CodeTag)
				{
					result.Append(childNode.ToBb(securitySubstitutions));
				}
                else if (Tag is PreformattedTag)
				{
				    result.Append(childNode.ToText(securitySubstitutions, null));
				}
				else
				{
					result.Append(childNode.ToText(securitySubstitutions, aliasSubstitutions));
				}
	        }
	        return result.ToString();
	    }

	    public override string ToBb(Dictionary<string, string> substitutions = null)
		{
			var result = new StringBuilder(Tag.WithAttribute && !string.IsNullOrEmpty(AttributeValue)
				? $@"[{Tag.Name}=""{AttributeValue}""]"
			    : $@"[{Tag.Name}]", ChildNodes.Count + 2);
			foreach (var childNode in ChildNodes)
			{
				result.Append(childNode.ToBb(substitutions));
			}
			if (Tag.RequiresClosing)
			{
				result.Append($@"[/{Tag.Name}]");
			}
			return result.ToString();
		}
	}
}