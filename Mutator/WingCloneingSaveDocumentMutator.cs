
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using PrisonArchitect.SaveEditor.SaveElements;

namespace PrisonArchitect.SaveEditor.Mutator
{
    internal sealed class WingCloneingSaveDocumentMutator : ISaveDocumentMutator
    {
        public WingCloneingSaveDocumentMutator(int cloneFromX,
                                               int cloneFromY,
                                               int cloneWidth,
                                               int cloneHeight,
                                               int cloneToX,
                                               int cloneToY)
        {
            m_CloneFromX  = cloneFromX;
            m_CloneFromY  = cloneFromY;
            m_CloneWidth  = cloneWidth;
            m_CloneHeight = cloneHeight;
            m_CloneToX    = cloneToX;
            m_CloneToY    = cloneToY;
        }

        public SaveDocument Mutate(SaveDocument saveDocument)
        {
            if (saveDocument == null)
            {
                throw new ArgumentNullException(nameof(saveDocument));
            }

            var nextObjetId = int.Parse
                (saveDocument.Sections.Single(s => s.Name == "Objects")
                                      .InnerPairs
                                      .Single(p => p.Key == "Size")
                                      .Value);

            saveDocument = ReplaceAreaWithCloneInSection("Cells", saveDocument);

            saveDocument = CloneObjects(saveDocument, nextObjetId);

            saveDocument = ReplaceAreaWithCloneInSection("Electricity",
                                                         saveDocument);

            saveDocument = ReplaceAreaWithCloneInSection("Water", saveDocument);

            saveDocument = ReplaceAreaWithCloneInSection("Patrols", saveDocument);

            return saveDocument;
        }

        private SaveDocument ReplaceAreaWithCloneInSection
            (string sectionName, SaveDocument saveDocument)
        {
            // This ended-up being a mess because I encountered parts of the
            // save file I hadn't spotted until the end, but I didn't feel
            // like refectoring, hence the work-around for other inner sections

            var section = saveDocument.Sections.Single
                (s => s.Name == sectionName);

            var regex = new Regex("\\d+ \\d+");

            var groupedInnerSections = section.InnerSections
                .GroupBy(s => regex.IsMatch(s.Name), s => s)
                .ToDictionary(g => g.Key, g => g.ToArray());

            if (!groupedInnerSections.ContainsKey(true))
            {
                groupedInnerSections[true] = new SaveSection[] {};
            }

            if (!groupedInnerSections.ContainsKey(false))
            {
                groupedInnerSections[false] = new SaveSection[] {};
            }

            section = new SaveSection
                (sectionName, groupedInnerSections[true], section.InnerPairs);

            saveDocument = RepaceSection(section, saveDocument);

            saveDocument = RemoveAreaInSection(sectionName, saveDocument);

            saveDocument = CloneAreaInSection(sectionName, saveDocument);

            section = saveDocument.Sections.Single(s => s.Name == sectionName);

            section = new SaveSection
                (sectionName,
                 section.InnerSections.Concat(groupedInnerSections[false]),
                 section.InnerPairs);

            return RepaceSection(section, saveDocument);
        }

        private SaveDocument RemoveAreaInSection(string sectionName,
                                                 SaveDocument saveDocument)
        {
            var section = saveDocument.Sections.Single
                (s => s.Name == sectionName);

            var newInnerSections = section.InnerSections
                .Select(GetContentsWithCoordinates)
                .Where(r => !IsInArea(r, m_CloneToX, m_CloneToY))
                .Select(r => new SaveSection($"{r.X} {r.Y}",
                                             r.InnerSections,
                                             r.InnerPairs))
                .ToArray();

            var newSection = new SaveSection
                (sectionName,
                 newInnerSections,
                 WithSizeCorrected(section.InnerPairs, newInnerSections.Length));

            return RepaceSection(newSection, saveDocument);
        }

        private SaveDocument RemoveObjectsInCloneToArea
            (SaveDocument saveDocument)
        {
            var section = saveDocument.Sections.Single
                (s => s.Name == "Objects");

            var newInnerSections = section.InnerSections
                .Select(GetObjectSectionWithCoordinates)
                .Where(r => !IsInArea(r, m_CloneToX, m_CloneToY))
                .Select(r => r.Section)
                .ToArray();

            var newSection = new SaveSection
                ("Objects",
                 newInnerSections,
                 WithSizeCorrected(section.InnerPairs, newInnerSections.Length));

            return RepaceSection(newSection, saveDocument);
        }

        private SaveDocument CloneObjects(SaveDocument saveDocument,
                                          int nextObjectId)
        {
            var nextUniqueId = long.Parse
                (saveDocument.OuterPairs.Single(p => p.Key == "ObjectId.next")
                                        .Value);

            var section = saveDocument.Sections.Single
                (s => s.Name == "Objects");

            var offsetX = m_CloneToX - m_CloneFromX;
            var offsetY = m_CloneToY - m_CloneFromY;

            var newInnerSections = section.InnerSections
                .Select(GetObjectSectionWithCoordinates)
                .Where(r => IsInArea(r, m_CloneFromX, m_CloneFromY))
                .Where(r => !m_ExcludedObjectTypes.Contains(r.Section.InnerPairs.Single(p => p.Key == "Type").Value))
                .Select(r =>
            {
                var objectId = nextObjectId++;

                var newInnerPairs = r.Section.InnerPairs;

                // ReSharper disable once AccessToModifiedClosure
                Func<string, double> getPairValue = key =>
                    double.Parse(newInnerPairs.Single(p => p.Key == key).Value);

                // ReSharper disable SpecifyACultureInStringConversionExplicitly
                newInnerPairs = ReplacePairValue
                    (newInnerPairs,
                     "Pos.x",
                     (getPairValue("Pos.x") + offsetX).ToString());

                newInnerPairs = ReplacePairValue
                    (newInnerPairs,
                     "Pos.y",
                     (getPairValue("Pos.y") + offsetY).ToString());
                // ReSharper restore SpecifyACultureInStringConversionExplicitly

                newInnerPairs = ReplacePairValue
                    (newInnerPairs, "Id.i", objectId.ToString());

                newInnerPairs = ReplacePairValue
                    (newInnerPairs, "Id.u", nextUniqueId++.ToString());

                return new SaveSection
                    ($"[i {objectId}]",
                     r.Section.InnerSections.Where(s => s.Name != "Connections"),
                     WithoutExcluded(newInnerPairs));
            })
            .ToArray();

            saveDocument = RemoveObjectsInCloneToArea(saveDocument);

            section = saveDocument.Sections.Single(s => s.Name == "Objects");

            var newSection = new SaveSection
                ("Objects",
                 section.InnerSections.Concat(newInnerSections).ToArray(),
                 WithSizeCorrected(section.InnerPairs, nextObjectId));
          
            saveDocument = RepaceSection(newSection, saveDocument);

            var outerPairs = ReplacePairValue(saveDocument.OuterPairs,
                                              "ObjectId.next",
                                              nextUniqueId.ToString());

            return new SaveDocument(outerPairs, saveDocument.Sections);
        }

        private SaveDocument CloneAreaInSection(string sectionName,
                                                SaveDocument saveDocument)
        {
            var section = saveDocument.Sections.Single
                (s => s.Name == sectionName);

            var offsetX = m_CloneToX - m_CloneFromX;
            var offsetY = m_CloneToY - m_CloneFromY;

            var newInnerSections = section.InnerSections
                .Select(GetContentsWithCoordinates)
                .Where(r => IsInArea(r, m_CloneFromX, m_CloneFromY))
                .Select(r => new SaveSection($"{r.X + offsetX} {r.Y + offsetY}",
                                             r.InnerSections,
                                             WithoutExcluded(r.InnerPairs)));

            var allInnerSections = section.InnerSections.Concat(newInnerSections)
                                                        .ToArray();
            var newSection = new SaveSection
                (sectionName,
                 allInnerSections,
                 WithSizeCorrected(section.InnerPairs, allInnerSections.Length));

            return RepaceSection(newSection, saveDocument);
        }

        private static SectionContentsWithCoordinates GetContentsWithCoordinates
            (SaveSection section)
        {
            var nameParts = section.Name.Split(' ');

            if (nameParts.Length != 2)
            {
                throw new Exception
                    ("More than one cell section name part");
            }

            return new SectionContentsWithCoordinates
                { InnerPairs    = section.InnerPairs,
                  InnerSections = section.InnerSections,
                  X             = int.Parse(nameParts[0]),
                  Y             = int.Parse(nameParts[1]) };
        }

        private static ObjectSectionWithCoordinates
            GetObjectSectionWithCoordinates(SaveSection section)
        {
            Func<string, double> parseIntPair = key =>
            {
                return double.Parse
                    (section.InnerPairs.Single(p => p.Key == key).Value);
            };

            return new ObjectSectionWithCoordinates
                { Section = section,
                  X       = parseIntPair("Pos.x"),
                  Y       = parseIntPair("Pos.y") };
        }

        private bool IsInArea(Coordinates coordinates, int x, int y)
        {
            return coordinates.X >= x
                && coordinates.X < x + m_CloneWidth
                && coordinates.Y >= y
                && coordinates.Y < y + m_CloneHeight;
        }

        private IEnumerable<SavePair> WithoutExcluded
            (IEnumerable<SavePair> pairs)
        {
            return pairs.Where(p => p.Key == "Id.i" || !p.Key.EndsWith(".i"))
                        .Where(p => p.Key == "Id.u" || !p.Key.EndsWith(".u"))
                        .Where(p => !m_ExcludedInnerPairKeys.Contains(p.Key));
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

        private static IReadOnlyCollection<SavePair> ReplacePairValue
            (IEnumerable<SavePair> savePairs, string key, string newValue)
        {
            var newSavePairs = savePairs.ToArray();

            var sizePairIndex = newSavePairs
                .Select((p, i) => new { p.Key, Index = i })
                .Single(r => r.Key == key)
                .Index;

            newSavePairs[sizePairIndex] = new SavePair(key, newValue);

            return newSavePairs;
        }

        private class Coordinates
        {
            public double X { get; set; }

            public double Y { get; set; }
        }

        private sealed class SectionContentsWithCoordinates : Coordinates
        {
            public IReadOnlyCollection<SavePair> InnerPairs { get; set; }

            public IReadOnlyCollection<SaveSection> InnerSections { get; set; }           
        }

        private sealed class ObjectSectionWithCoordinates : Coordinates
        {
            public SaveSection Section { get; set; }
        }

        private readonly int m_CloneFromX;
        private readonly int m_CloneFromY;
        private readonly int m_CloneWidth;
        private readonly int m_CloneHeight;
        private readonly int m_CloneToX;
        private readonly int m_CloneToY;

        private readonly IReadOnlyCollection<string> m_ExcludedObjectTypes = new []
        {
            "Accountant",
            "ArmedGuard",
            "Box",
            "Chief",
            "Cook",
            "CrumpledPrisonerUniform",
            "DirtyPrisonerUniform",
            "Doctor",
            "Dog",
            "DogHandler",
            "Dummy",
            "FoodTrayDirty",
            "Foreman",
            "Garbage",
            "Gardener",
            "Guard",
            "Ingredients",
            "Janitor",
            "LaundryBasket",
            "Lawyer",
            "LibraryBookUnsorted",
            "Log",
            "MailSatchel",
            "Prisoner",
            "PrisonerUniform",
            "Psychologist",
            "ShopGoods",
            "Stack",
            "SupplyTruck",
            "TreeStump",
            "TruckDriver",
            "WaterPumpStation",
            "Warden",
            "Wood",
            "Workman"
        };

        private readonly IReadOnlyCollection<string> m_ExcludedInnerPairKeys = new []
        {
            "JobId",
            "Damage",
            "LastPress",
            "StoredObject",
            "NumBooks",
            "Occupied",
            "Stock"
        };
    }
}