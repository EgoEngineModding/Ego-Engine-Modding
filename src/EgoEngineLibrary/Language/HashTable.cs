namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class HashTable : IEnumerable
    {
        private int Bucket;
        private List<HashEntry> Entry;
        public int Count
        {
            get { return Entry.Count; }
        }

        public HashTable()
        {
            Bucket = 0;
            Entry = new List<HashEntry>();
        }
        public HashTable(int bucket, int count)
        {
            Bucket = bucket;
            Entry = new List<HashEntry>(count);
        }

        public void Add(HashEntry entry)
        {
            Entry.Add(entry);
        }
        public void RemoveAt(int index)
        {
            Entry.RemoveAt(index);
        }

        public HashEntry this[int index]
        {
            get { return Entry[index]; }
            set { Entry[index] = value; }
        }

        public IEnumerator<HashEntry> GetEnumerator()
        {
            foreach (HashEntry entry in Entry)
            {
                yield return entry;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
