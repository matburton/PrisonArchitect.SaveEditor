
using System.IO;

using PrisonArchitect.SaveEditor.Converters;
using PrisonArchitect.SaveEditor.Mutator;

namespace PrisonArchitect.SaveEditor
{
    public static class Program
    {
        public static int Main(string[] arguments)
        {
            if (arguments.Length != 2) return 1;

            var saveDocumentString = File.ReadAllText(arguments[0]);

            var saveDocumentConverter = new SaveDocumentConverter();

            var saveDocument = saveDocumentConverter.Parse(saveDocumentString);

            var saveDocumentMutator = new MapShiftingSaveDocumentMutator(0, 0);

            saveDocument = saveDocumentMutator.Mutate(saveDocument);

            saveDocumentString = saveDocumentConverter.Persist(saveDocument);

            File.WriteAllText(arguments[1], saveDocumentString);

            return 0;
        }
    }
}