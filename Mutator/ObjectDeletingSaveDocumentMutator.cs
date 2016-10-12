
using System;
using System.Collections.Generic;
using System.Linq;

using PrisonArchitect.SaveEditor.SaveElements;

namespace PrisonArchitect.SaveEditor.Mutator
{
    internal sealed class ObjectDeletingSaveDocumentMutator : ISaveDocumentMutator
    {
        public ObjectDeletingSaveDocumentMutator(string objectType)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            m_ObjectType = objectType;
        }

        public SaveDocument Mutate(SaveDocument saveDocument)
        {
            if (saveDocument == null)
            {
                throw new ArgumentNullException(nameof(saveDocument));
            }
            
            var section = saveDocument.Sections.Single
                (s => s.Name == "Objects");

            var newInnerSections = section.InnerSections
                .Where(s => !IsObjectOfType(s, m_ObjectType)
                         && !IsObjectContainingType(s, m_ObjectType))
                .ToArray();

            var newSection = new SaveSection
                ("Objects",
                 newInnerSections,
                 WithSizeCorrected(section.InnerPairs, newInnerSections.Length));

            return RepaceSection(newSection, saveDocument);
        }

        private static bool IsObjectOfType(SaveSection section,
                                           string objectType)
        {
            return section.InnerPairs.Single(p => p.Key == "Type").Value
                == objectType;
        }

        private static bool IsObjectContainingType(SaveSection section,
                                                   string objectType)
        {
            var contentsPair = section.InnerPairs.SingleOrDefault
                (p => p.Key == "Contents");

            return contentsPair?.Value == objectType;
        }

        private static IEnumerable<SavePair> WithSizeCorrected
            (IReadOnlyCollection<SavePair> savePairs, int newSize)
        {
            var newSavePairs = savePairs.ToArray();

            var sizePairIndex = newSavePairs
                .Select((p, i) => new { p.Key, Index = i })
                .SingleOrDefault(r => r.Key == "Size")
                ?.Index;

            if (sizePairIndex == null) return savePairs;

            newSavePairs[sizePairIndex.Value] =
                new SavePair("Size", newSize.ToString());

            return newSavePairs;
        }

        private static SaveDocument RepaceSection(SaveSection newSection,
                                                  SaveDocument saveDocument)
        {
            var sections = saveDocument.Sections.ToArray();

            var sectionIndex = sections.Select((s, i) => new { s, Index = i })
                                       .Single(r => r.s.Name == newSection.Name)
                                       .Index;

            sections[sectionIndex] = newSection;

            return new SaveDocument(saveDocument.OuterPairs, sections);
        }

        private readonly string m_ObjectType;
    }
}
