﻿using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.MarkDown.Yaml
{
    public sealed class TerminalNode : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "span", Order = 4)]
        public CharacterSpan Span { get; set; }

        public override CharacterSpan GetTotalSpan() => Span;

        public override TerminalNode ToTerminalNode() => this;
    }
}