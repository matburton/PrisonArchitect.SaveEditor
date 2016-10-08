
using System;

using PrisonArchitect.SaveEditor.SaveElements;

namespace PrisonArchitect.SaveEditor.Converters
{
    internal sealed class DocumentConverter
    {
        public RawSaveFile Parse(string saveDocument)
        {
            if (saveDocument == null)
            {
                throw new ArgumentNullException(nameof(saveDocument));
            }

            throw new NotImplementedException();
        }

        public string Persist(RawSaveFile saveFile)
        {
            if (saveFile == null)
            {
                throw new ArgumentNullException(nameof(saveFile));
            }

            throw new NotImplementedException();
        }
    }
}