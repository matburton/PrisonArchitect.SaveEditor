
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonArchitect.SaveEditor.SaveElements
{
    internal sealed class SaveDocument
    {
        public SaveDocument(IEnumerable<SavePair> outerPairs,
                            IEnumerable<SaveSection> sections)
        {
            if (outerPairs == null)
            {
                throw new ArgumentNullException(nameof(outerPairs));
            }

            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            OuterPairs = outerPairs.ToArray();

            Sections = sections.ToArray();

            if (OuterPairs.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(outerPairs));
            }

            if (Sections.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(sections));
            }
        }

        public IReadOnlyCollection<SavePair> OuterPairs { get; }

        public IReadOnlyCollection<SaveSection> Sections { get; }
    }
}