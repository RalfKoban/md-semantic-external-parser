﻿using System.IO;

using MiKoSolutions.SemanticParsers.MarkDown.Yaml.Converters;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.MarkDown.Yaml
{
    public static class YamlWriter
    {
        public static void Write(TextWriter writer, object graph)
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new CharacterSpanConverter())
                .WithTypeConverter(new LocationSpanConverter())
                .WithTypeConverter(new ParsingErrorConverter())
                .EmitDefaults() // Force even default values to be written, like 0, false.
                .Build();

            serializer.Serialize(writer, graph);
        }
    }
}