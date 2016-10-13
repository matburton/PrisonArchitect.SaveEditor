
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PrisonArchitect.SaveEditor.SaveElements;

namespace PrisonArchitect.SaveEditor.Mutator
{
    internal sealed class MapShiftingSaveDocumentMutator : ISaveDocumentMutator
    {
        public MapShiftingSaveDocumentMutator(int lengthX, int lengthY)
        {
            if (m_LengthX > 0 || m_LengthY > 0)
            {
                throw new NotImplementedException("Cannot increase prison size");
            }

            m_LengthX = lengthX;

            m_LengthY = lengthY;
        }

        public SaveDocument Mutate(SaveDocument saveDocument)
        {
            var outerPairs = saveDocument.OuterPairs.ToDictionary
                (p => p.Key, p => p.Value);

            outerPairs["NumCellsX"] = (int.Parse(outerPairs["NumCellsX"]) + m_LengthX).ToString();
            outerPairs["NumCellsY"] = (int.Parse(outerPairs["NumCellsY"]) + m_LengthY).ToString();

            outerPairs["OriginW"] = (int.Parse(outerPairs["OriginW"]) + m_LengthX).ToString();
            outerPairs["OriginH"] = (int.Parse(outerPairs["OriginH"]) + m_LengthY).ToString();

            return new SaveDocument
                (outerPairs.Select(p => new SavePair(p.Key, p.Value)),
                 ShiftEverything(saveDocument.Sections));
        }

        private IEnumerable<SaveSection> ShiftEverything
            (IEnumerable<SaveSection> sections)
        {
            var regex = new Regex("\\d+ \\d+");

            foreach (var section in sections)
            {
                var sectionName = section.Name;

                if (regex.IsMatch(sectionName))
                {
                    var nameParts = sectionName.Split(' ');

                    if (nameParts.Length != 2)
                    {
                        throw new Exception
                            ("More than one cell section name part");
                    }

                    var x = int.Parse(nameParts[0]) + m_LengthX;
                    var y = int.Parse(nameParts[1]) + m_LengthY;

                    if (x < 0 || y < 0) continue;

                    sectionName = $"{x} {y}";
                }

                yield return new SaveSection
                    (sectionName,
                     ShiftEverything(section.InnerSections),
                     section.InnerPairs.Select(ShiftEverything));
            }
        }

        private SavePair ShiftEverything(SavePair savePair)
        {
            // ReSharper disable SimplifyLinqExpression
            if (   savePair.Key.EndsWith(".x")
                && !m_ExcludedPairKeys.Any(k => $"{k}.x" == savePair.Key))
            {
                return ShiftBy(-18, savePair);
            }

            if (   savePair.Key.EndsWith(".y")
                && !m_ExcludedPairKeys.Any(k => $"{k}.y" == savePair.Key))
            {
                return ShiftBy(-25, savePair);
            }
            // ReSharper restore SimplifyLinqExpression

            return savePair;
        }

        private static SavePair ShiftBy(int length, SavePair savePair)
        {
            var separatorIndex = savePair.Value.IndexOf('.');

            var integer = int.Parse
                (separatorIndex == -1
                    ? savePair.Value
                    : savePair.Value.Substring(0, separatorIndex));

            integer = integer + length;

            if (integer < 0) integer = 0;

            var postfix = string.Empty;

            if (separatorIndex == -1)
            {
                return new SavePair(savePair.Key, $"{integer}{postfix}");
            }

            postfix = savePair.Value.Substring
                (separatorIndex + 1,
                 savePair.Value.Length - separatorIndex - 1);

            postfix = $".{postfix}";

            return new SavePair(savePair.Key, $"{integer}{postfix}");
        }

        private readonly int m_LengthX;

        private readonly int m_LengthY;

        private readonly IReadOnlyCollection<string> m_ExcludedPairKeys =
            new [] {"Or", "Walls", "Vel", "OpenDir"};
    }
}