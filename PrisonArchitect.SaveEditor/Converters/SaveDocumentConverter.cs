
using System;
using System.Collections.Generic;
using System.Text;

using PrisonArchitect.SaveEditor.SaveElements;

namespace PrisonArchitect.SaveEditor.Converters
{
    internal sealed class SaveDocumentConverter
    {
        public SaveDocument Parse(string saveDocumentString)
        {
            if (saveDocumentString == null)
            {
                throw new ArgumentNullException(nameof(saveDocumentString));
            }

            IEnumerable<string> persistedParts =
                saveDocumentString.Split(new [] { ' ', '\r', '\n', '\t' },
                                         StringSplitOptions.RemoveEmptyEntries);

            var partsEnumerator = persistedParts.GetEnumerator();

            var parseResult = Parse(partsEnumerator);

            return new SaveDocument(parseResult.SavePairs,
                                    parseResult.SaveSections);
        }

        public string Persist(SaveDocument saveDocument)
        {
            if (saveDocument == null)
            {
                throw new ArgumentNullException(nameof(saveDocument));
            }

            ICollection<string> persistedParts = new LinkedList<string>();

            PersistPairs(saveDocument.OuterPairs, persistedParts);

            PersistSections(saveDocument.Sections, persistedParts);

            return string.Join(" ", persistedParts);
        }

        private static ParseResult Parse(IEnumerator<string> partsEnumerator)
        {
            var savePairs = new List<SavePair>();

            var saveSections = new List<SaveSection>();

            while (   partsEnumerator.MoveNext()
                   && partsEnumerator.Current != "END")
            {
                var nextPart = partsEnumerator.Current;

                if (nextPart == "BEGIN")
                {
                    if (!partsEnumerator.MoveNext())
                    {
                        throw new Exception("Expected section name after"
                                            + " BEGIN but document ended");
                    }

                    var sectionName = ParsePart(partsEnumerator);

                    var parseResult = Parse(partsEnumerator);

                    if (partsEnumerator.Current != "END")
                    {
                        throw new Exception
                            ("Expected END to close section but got"
                             + $" part '{partsEnumerator.Current}'");
                    }

                    saveSections.Add(new SaveSection(sectionName,
                                                     parseResult.SaveSections,
                                                     parseResult.SavePairs));
                }
                else
                {
                    var key = ParsePart(partsEnumerator);

                    if (!partsEnumerator.MoveNext())
                    {
                        throw new Exception
                            ("Expected pair value but document ended");
                    }

                    savePairs.Add(new SavePair(key, ParsePart(partsEnumerator)));
                }
            }

            return new ParseResult { SavePairs    = savePairs,
                                     SaveSections = saveSections };
        }

        private static string ParsePart(IEnumerator<string> partsEnumerator)
        {
            if (!partsEnumerator.Current.StartsWith("\""))
            {
                return partsEnumerator.Current;
            }

            var stringBuilder = new StringBuilder(partsEnumerator.Current);

            while (!partsEnumerator.Current.EndsWith("\""))
            {
                if (!partsEnumerator.MoveNext())
                {
                    throw new Exception("Reached end of document when trying to"
                                        + " find part to complete quoted part");
                }

                stringBuilder.Append($" {partsEnumerator.Current}");
            }

            var quotedPart = stringBuilder.ToString();

            return quotedPart.Substring(1, quotedPart.Length - 2);
        }

        private static void PersistPairs(IEnumerable<SavePair> pairs,
                                         ICollection<string> persistedParts)
        {
            foreach (var pair in pairs)
            {
                persistedParts.Add($"{pair.Key} {pair.Value}");
            }
        }

        private static void PersistSections(IEnumerable<SaveSection> sections,
                                            ICollection<string> persistedParts)
        {
            foreach (var section in sections)
            {
                PersistSection(section, persistedParts);
            }
        }

        private static void PersistSection(SaveSection section,
                                           ICollection<string> persistedParts)
        {
            var sectionName = section.Name.Contains(" ") ? $"\"{section.Name}\""
                                                         : section.Name;

            persistedParts.Add($"BEGIN {sectionName}");

            PersistPairs(section.InnerPairs, persistedParts);

            PersistSections(section.InnerSections, persistedParts);

            persistedParts.Add("END");
        }

        private sealed class ParseResult
        {
            public IReadOnlyCollection<SavePair> SavePairs { get; set; }

            public IReadOnlyCollection<SaveSection> SaveSections { get; set; }
        }
    }
}