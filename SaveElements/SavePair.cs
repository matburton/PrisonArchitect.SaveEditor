
using System;

namespace PrisonArchitect.SaveEditor.SaveElements
{
    internal sealed class SavePair
    {
        public SavePair(string key, string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (value == null) throw new ArgumentNullException(nameof(value));

            Key = key;

            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }
}