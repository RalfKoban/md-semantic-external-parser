using System;
using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.MarkDown.Yaml;

using NUnit.Framework;

using File = MiKoSolutions.SemanticParsers.MarkDown.Yaml.File;

using NUnitAssert = NUnit.Framework.Assert;

namespace MiKoSolutions.SemanticParsers.MarkDown
{
    [TestFixture]
    public class ParserTests
    {
        private string _resourceDirectory;

        [SetUp]
        public void PrepareTest()
        {
            var directory = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            _resourceDirectory = Path.Combine(directory, "Resources");
        }

        [Test]
        public void Parse_Heading_Paragraph()
        {
            var file = Parser.Parse(Path.Combine(_resourceDirectory, "Heading1_TextParagraph.md"));

            var yaml = CreateYaml(file);

            NUnitAssert.Multiple(() =>
            {
                NUnitAssert.That(file.Children.Count, Is.EqualTo(1), yaml);

                var child = file.Children.Single();
                NUnitAssert.That(child.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Root start wrong");
                NUnitAssert.That(child.LocationSpan.End, Is.EqualTo(new LineInfo(6, 10)), "Root end wrong");

                Assert<TerminalNode>("Headline 1", child.Children[0], new LineInfo(1, 1), new LineInfo(1, 12), new CharacterSpan(0, 11));

                // TODO: RKN add missing tests
            });
        }

        [Test]
        public void Parse_Heading_ListBlock()
        {
            var file = Parser.Parse(Path.Combine(_resourceDirectory, "Heading1_ListBlock.md"));

            var yaml = CreateYaml(file);

            NUnitAssert.Multiple(() =>
            {
                NUnitAssert.That(file.Children.Count, Is.EqualTo(1), yaml);

                var child = file.Children.Single();
                NUnitAssert.That(child.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Root start wrong");
                NUnitAssert.That(child.LocationSpan.End, Is.EqualTo(new LineInfo(6, 14)), "Root end wrong");

                Assert<TerminalNode>("Headline 1", child.Children[0], new LineInfo(1, 1), new LineInfo(1, 28), new CharacterSpan(0, 27));

                var list = Assert<Container>("ListBlock 1", child.Children[1], new LineInfo(2, 1), new LineInfo(6, 14), new CharacterSpan(28, 127));

                Assert<Container>("ListItem 1", list.Children[0], new LineInfo(2, 1), new LineInfo(2, 16), new CharacterSpan(28, 43));
                Assert<Container>("ListItem 2", list.Children[1], new LineInfo(3, 1), new LineInfo(5, 21), new CharacterSpan(44, 113));
                Assert<Container>("ListItem 3", list.Children[2], new LineInfo(6, 1), new LineInfo(6, 14), new CharacterSpan(114, 127));

                // TODO: RKN add missing tests
            });
        }

        private static T Assert<T>(string name, ContainerOrTerminalNode node, LineInfo start, LineInfo end, CharacterSpan totalSpan) where T : ContainerOrTerminalNode
        {
            NUnitAssert.That(node.LocationSpan.Start, Is.EqualTo(start), name + " start wrong");
            NUnitAssert.That(node.LocationSpan.End, Is.EqualTo(end), name + " end wrong");
            NUnitAssert.That(node.GetTotalSpan(), Is.EqualTo(totalSpan), name + " span wrong");

            return (T)node;
        }

        private static string CreateYaml(File file)
        {
            var builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                YamlWriter.Write(writer, file);
            }

            var yaml = builder.ToString();
            return yaml;
        }
    }
}
