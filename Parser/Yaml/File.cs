﻿using System.Collections.Generic;
using System.Linq;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.MarkDown.Yaml
{
    public sealed class File
    {
        [YamlMember(Alias = "type", Order = 1)]
        public string Type { get; } = "file";

        [YamlMember(Alias = "name", Order = 2)]
        public string Name { get; set; }

        [YamlMember(Alias = "locationSpan", Order = 3)]
        public LocationSpan LocationSpan { get; set; }

        [YamlMember(Alias = "footerSpan", Order = 4)]
        public CharacterSpan FooterSpan { get; set; }

        [YamlMember(Alias = "children", Order = 7)]
        public List<Container> Children { get; } = new List<Container>();

        [YamlMember(Alias = "parsingErrorsDetected", Order = 5)]
        public bool? ParsingErrorsDetected => ParsingErrors.Any();

        [YamlMember(Alias = "parsingError", Order = 6)]
        public List<ParsingError> ParsingErrors { get; } = new List<ParsingError>();
    }
}