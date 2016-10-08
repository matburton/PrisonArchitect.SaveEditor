
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonArchitect.SaveEditor.SaveElements
{
    internal sealed class RawSaveSection
    {
        public RawSaveSection(string name,
                              IEnumerable<RawSaveSection> innerSections,
                              IEnumerable<SavePair> innerValues)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (innerSections == null)
            {
                throw new ArgumentNullException(nameof(innerSections));
            }

            if (innerValues == null)
            {
                throw new ArgumentNullException(nameof(innerValues));
            }

            Name = name;

            InnerSections = innerSections.ToArray();

            InnerValues = innerValues.ToArray();

            if (InnerSections.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(innerSections));
            }

            if (InnerValues.Contains(null))
            {
                throw new ArgumentException("Contained null", nameof(innerValues));
            }
        }

        public string Name { get; }

        public IReadOnlyCollection<RawSaveSection> InnerSections { get; }

        public IReadOnlyCollection<SavePair> InnerValues { get; }
    }
}