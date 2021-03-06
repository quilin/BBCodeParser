﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BBCodeParser.Nodes
{
    public class TextNode : Node
    {
        private readonly string text;

        public TextNode(string text)
        {
            this.text = text;
        }

        private static string SubstituteText(string text, Dictionary<string, string> substitutions)
        {
            return substitutions == null
                ? text
                : substitutions.Aggregate(text,
                    (current, substitution) => current.Replace(substitution.Key, substitution.Value));
        }

        internal override string ToHtml(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions,
            Func<Node, bool> filter = null,
            Func<Node, string, string> filterAttributeValue = null)
        {
            return SubstituteText(SubstituteText(text, securitySubstitutions), aliasSubstitutions);
        }

        internal override string ToText(
            Dictionary<string, string> securitySubstitutions,
            Dictionary<string, string> aliasSubstitutions,
            Func<Node, bool> filter = null,
            Func<Node, string, string> filterAttributeValue = null
        )
        {
            return SubstituteText(SubstituteText(text, securitySubstitutions), aliasSubstitutions);
        }

        internal override string ToBb(
            Dictionary<string, string> securitySubstitutions,
            Func<Node, bool> filter = null,
            Func<Node, string, string> filterAttributeValue = null)
        {
            return SubstituteText(text, securitySubstitutions);
        }

        public override void AddChild(Node node)
        {
            throw new NotImplementedException();
        }
    }
}