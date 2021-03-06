﻿using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.MarkDown.Yaml
{
    public sealed class Container : ContainerOrTerminalNode
    {
        [YamlMember(Alias = "headerSpan", Order = 4)]
        public CharacterSpan HeaderSpan { get; set; }

        [YamlMember(Alias = "footerSpan", Order = 5)]
        public CharacterSpan FooterSpan { get; set; }

        [YamlMember(Alias = "children", Order = 6)]
        public List<ContainerOrTerminalNode> Children { get; } = new List<ContainerOrTerminalNode>();

        public override CharacterSpan GetTotalSpan() => new CharacterSpan(HeaderSpan.Start, FooterSpan.End);

        public override TerminalNode ToTerminalNode() => new TerminalNode
                                                             {
                                                                 Type = Type,
                                                                 Name = Name,
                                                                 LocationSpan = LocationSpan,
                                                                 Span = GetTotalSpan(),
                                                             };
    }
}