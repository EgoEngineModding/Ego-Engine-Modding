namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class HashEntry
    {
        public string Key;
        public string Value;

        public HashEntry()
        {
            Key = string.Empty;
            Value = string.Empty;
        }
        public HashEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
