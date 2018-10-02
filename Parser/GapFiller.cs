using System.Collections.Generic;

using MiKoSolutions.SemanticParsers.MarkDown.Yaml;

namespace MiKoSolutions.SemanticParsers.MarkDown
{
    public static class GapFiller
    {
        public static void Fill(File file, CharacterPositionFinder finder)
        {
            foreach (var root in file.Children)
            {
                AdjustChildren(root, finder);
            }
        }

        private static void AdjustChildren(Container container, CharacterPositionFinder finder)
        {
            var children = container.Children;

            for (var index = 0; index < children.Count; index++)
            {
                AdjustNode(children, index, finder);
            }
        }

        private static void AdjustNode(IList<ContainerOrTerminalNode> parentChildren, int indexInParentChildren, CharacterPositionFinder finder)
        {
            if (indexInParentChildren < parentChildren.Count - 1)
            {
                var child = parentChildren[indexInParentChildren];
                var nextSibling = parentChildren[indexInParentChildren + 1];

                var indexBefore = finder.GetCharacterPosition(nextSibling.LocationSpan.Start) - 1;
                var newEndPos = finder.GetLineInfo(indexBefore);

                child.LocationSpan = new LocationSpan(child.LocationSpan.Start, newEndPos);

                if (child is Container c)
                {
                    AdjustChildren(c, finder);
                }
                else if (child is TerminalNode t)
                {
                    var start = finder.GetCharacterPosition(t.LocationSpan.Start);
                    var end = finder.GetCharacterPosition(t.LocationSpan.End);
                    t.Span = new CharacterSpan(start, end);
                }
            }
        }
    }
}