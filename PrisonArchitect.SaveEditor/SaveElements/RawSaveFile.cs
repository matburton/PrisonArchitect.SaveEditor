
using System;
using System.Collections.Generic;
using System.Linq;

namespace PrisonArchitect.SaveEditor.SaveElements
{
    internal sealed class RawSaveFile
    {
        public RawSaveFile(IEnumerable<SavePair> topLevelValues,
                           IEnumerable<RawSaveSection> saveSections)
        {
            if (topLevelValues == null)
            {
                throw new ArgumentNullException(nameof(topLevelValues));
            }

            if (saveSections == null)
            {
                throw new ArgumentNullException(nameof(saveSections));
            }

            TopLevelValues = topLevelValues.ToArray();

            SaveSections = saveSections.ToArray();

            if (TopLevelValues.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(topLevelValues));
            }

            if (SaveSections.Contains(null))
            {
                throw new ArgumentException("Contained null",
                                            nameof(saveSections));
            }
        }

        public IReadOnlyCollection<SavePair> TopLevelValues { get; }

        public IReadOnlyCollection<RawSaveSection> SaveSections { get; }
    }
}