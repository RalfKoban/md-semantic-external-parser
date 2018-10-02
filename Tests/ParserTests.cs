using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.MarkDown.Yaml;

using NUnit.Framework;

using File = MiKoSolutions.SemanticParsers.MarkDown.Yaml.File;

namespace MiKoSolutions.SemanticParsers.MarkDown
{
    [TestFixture]
    public class ParserTests
    {
        [TestCase("Markdown_Heading1_TextParagraph.md")]
        public void Parse(string fileName)
        {
            var directory = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            var path = Path.Combine(directory, "Resources", fileName);

            var file = Parser.Parse(path);

            var yaml = CreateYaml(file);

            Assert.Multiple(() =>
            {
                Assert.That(file.Children.Count, Is.EqualTo(1), yaml);

                var child = file.Children.Single();
                Assert.That(child.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Root start wrong");
                Assert.That(child.LocationSpan.End, Is.EqualTo(new LineInfo(6, 10)), "Root end wrong");

                var headLine1 = child.Children[0];
                Assert.That(headLine1.LocationSpan.Start, Is.EqualTo(new LineInfo(1, 1)), "Headline 1 start wrong");
                Assert.That(headLine1.LocationSpan.End, Is.EqualTo(new LineInfo(1, 12)), "Headline 1 end wrong");
                Assert.That(headLine1.GetTotalSpan(), Is.EqualTo(new CharacterSpan(0, 11)), "Headline 1 span wrong");

                // TODO: RKN add missing tests
            });
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
