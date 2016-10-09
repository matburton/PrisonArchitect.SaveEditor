
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonArchitect.SaveEditor.SaveElements
{
    internal sealed class SaveSection
    {
        public SaveSection(string name,
                           IEnumerable<SaveSection> innerSections,
                           IEnumerable<SavePair> innerPairs)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (innerSections == null)
            {
                throw new ArgumentNullException(nameof(innerSections));
            }

            if (innerPairs == null)
            {
                throw new ArgumentNullException(nameof(innerPairs));
            }

            Name = name;

            InnerSections = innerSections.ToArray();

            InnerPairs = innerPairs.ToArray();

            if (InnerSections.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(innerSections));
            }

            if (InnerPairs.Contains(null))
            {
                throw new ArgumentException("Contained null", nameof(innerPairs));
            }
        }

        public string Name { get; }

        public IReadOnlyCollection<SaveSection> InnerSections { get; }

        public IReadOnlyCollection<SavePair> InnerPairs { get; }
    }
}