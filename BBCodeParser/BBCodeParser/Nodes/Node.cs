using System;
using System.Collections.Generic;

namespace BBCodeParser.Nodes
{
	public abstract class Node
	{
		protected List<Node> ChildNodes { get; set; }
		internal Node ParentNode { get; set; }

		internal abstract string ToHtml(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions,
            Func<Node, bool> filter = null,
            Func<Node, string, string> filterAttributeValue = null);
	    internal abstract string ToText(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions
            );
		public abstract string ToBb(Dictionary<string, string> substitutions = null);

		public virtual void AddChild(Node node)
		{
			ChildNodes.Add(node);
		}
	}
}