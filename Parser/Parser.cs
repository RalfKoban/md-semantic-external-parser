using System;
using System.Linq;
using System.Text;

using Markdig;
using Markdig.Parsers;
using Markdig.Syntax;

using MiKoSolutions.SemanticParsers.MarkDown.Yaml;

using Container = MiKoSolutions.SemanticParsers.MarkDown.Yaml.Container;
using File = MiKoSolutions.SemanticParsers.MarkDown.Yaml.File;
using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.MarkDown
{
    public static class Parser
    {
        // we have issues with UTF-8 encodings in files that should have an encoding='iso-8859-1'
        public static File Parse(string filePath) => Parse(filePath, "iso-8859-1");

        public static File Parse(string filePath, string encoding)
        {
            var encodingToUse = Encoding.GetEncoding(encoding);

            File file;
            using (var finder = CharacterPositionFinder.CreateFrom(filePath, encodingToUse))
            {
                file = ParseCore(filePath, finder, encodingToUse);

                Resorter.Resort(file);

                GapFiller.Fill(file, finder);
            }

            return file;
        }

        public static File ParseCore(string filePath, CharacterPositionFinder finder, Encoding encoding)
        {
            var text = SystemFile.ReadAllText(filePath, encoding);

            var file = new File
                           {
                               Name = filePath,
                               FooterSpan = new CharacterSpan(0, -1), // there is no footer
                           };

            try
            {
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var document = MarkdownParser.Parse(text, pipeline);

                var textProvider = new TextProvider(text);

                var locationSpan = GetLocationSpan(document, finder);

                var rootBlock = new Container
                                    {
                                        Type = "markdown",
                                        Name = string.Empty,
                                        LocationSpan = locationSpan,
                                        HeaderSpan = new CharacterSpan(0, 0), // there is no header
                                        FooterSpan = new CharacterSpan(text.Length - 1, text.Length - 1), // there is no footer
                                    };
                file.Children.Add(rootBlock);

                // Parse the file and display the text content of each of the elements.
                var blocks = document.Where(_ => !_.Span.IsEmpty).ToList();
                foreach (var block in blocks)
                {
                    var parsedBlock = ParseBlock(block, finder, textProvider);
                    rootBlock.Children.Add(parsedBlock);
                }

                file.LocationSpan = locationSpan;
            }
            catch (Exception ex)
            {
                // try to adjust location span to include full file content
                // but ignore empty files as parsing errors
                var lines = SystemFile.ReadLines(filePath).Count();
                if (lines == 0)
                {
                    file.LocationSpan = new LocationSpan(new LineInfo(0, -1), new LineInfo(0, -1));
                }
                else
                {
                    file.ParsingErrors.Add(new ParsingError
                                               {
                                                   ErrorMessage = ex.Message,
                                                   Location = new LineInfo(0, -1),
                                               });

                    file.LocationSpan = new LocationSpan(new LineInfo(1, 0), new LineInfo(lines + 1, 0));
                }
            }

            return file;
        }

        private static ContainerOrTerminalNode ParseBlock(Block block, CharacterPositionFinder finder, TextProvider textProvider)
        {
            switch (block)
            {
                case LeafBlock leaf:
                    return ParseBlock(leaf, finder, textProvider);

                case ContainerBlock container:
                    return ParseBlock(container, finder, textProvider);

                default:
                    throw new NotSupportedException("unknown block: " + block.GetType().Name);
            }
        }

        private static TerminalNode ParseBlock(LeafBlock block, CharacterPositionFinder finder, TextProvider textProvider)
        {
            var name = GetName(block, textProvider);
            var type = GetType(block);
            var locationSpan = GetLocationSpan(block, finder);
            var span = GetCharacterSpan(block);

            return new TerminalNode
                       {
                           Type = type,
                           Name = name,
                           LocationSpan = locationSpan,
                           Span = span,
                       };
        }

        private static ContainerOrTerminalNode ParseBlock(ContainerBlock block, CharacterPositionFinder finder, TextProvider textProvider)
        {
            var name = GetName(block, textProvider);
            var type = GetType(block);
            var locationSpan = GetLocationSpan(block, finder);

            var container = new Container
                                {
                                    Type = type,
                                    Name = name,
                                    LocationSpan = locationSpan,
                                    HeaderSpan = GetHeaderSpan(block),
                                    FooterSpan = GetFooterSpan(block),
                                };

            if (block is ListBlock list)
            {
                foreach (var listItem in list)
                {
                    var item = ParseBlock(listItem, finder, textProvider);
                    container.Children.Add(item);
                }
            }

            // TODO: RKN
            // - Table
            // - ListBlock

            // check whether we can use a terminal node instead
            var child = FinalAdjustAfterParsingComplete(container);

            return child;
        }

        private static LocationSpan GetLocationSpan(MarkdownObject mdo, CharacterPositionFinder finder)
        {
            var span = mdo.Span;
            var start = finder.GetLineInfo(span.Start);
            var end = finder.GetLineInfo(span.End);

            return new LocationSpan(start, end);
        }

        private static CharacterSpan GetCharacterSpan(MarkdownObject mdo) => new CharacterSpan(mdo.Span.Start, mdo.Span.End);

        private static CharacterSpan GetHeaderSpan(MarkdownObject mdo) => new CharacterSpan(mdo.Span.Start, mdo.Span.Start); // TODO: RKN + mdo.Span.Length);

        private static CharacterSpan GetFooterSpan(MarkdownObject mdo) => new CharacterSpan(mdo.Span.End, mdo.Span.End);

        private static string GetName(MarkdownObject mdo, TextProvider textProvider)
        {
            switch (mdo)
            {
                case HeadingBlock heading:
                    var text = textProvider.GetText(heading);
                    var name = text.Trim(heading.HeaderChar).Trim();
                    return name;

                default:
                    return string.Empty; // TODO: RKN mdo.GetType().Name;
            }
        }

        private static string GetType(MarkdownObject mdo)
        {
            var type = mdo.GetType().Name;

            return mdo is HeadingBlock heading
                    ? type + " " + heading.Level
                    : type;
        }

        private static ContainerOrTerminalNode FinalAdjustAfterParsingComplete(Container container) => container;

        private sealed class TextProvider
        {
            private readonly string _text;

            public TextProvider(string text) => _text = text;

            public string GetText(MarkdownObject mdo) => GetText(mdo.Span);

            public string GetText(SourceSpan span) => _text.Substring(span.Start, span.Length);
        }
    }
}