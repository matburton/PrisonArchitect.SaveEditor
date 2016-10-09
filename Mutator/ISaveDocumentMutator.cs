
using PrisonArchitect.SaveEditor.SaveElements;

namespace PrisonArchitect.SaveEditor.Mutator
{
    internal interface ISaveDocumentMutator
    {
        SaveDocument Mutate(SaveDocument saveDocument);
    }
}