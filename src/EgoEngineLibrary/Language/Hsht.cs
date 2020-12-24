namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Hsht : IEnumerable
    {
        public int Magic;
        private int Size;
        public HashTable[] Table;

        public Hsht(LngBinaryReader b)
        {
            Magic = b.ReadInt32();
            Size = b.ReadInt32();
            Table = new HashTable[Size / 8];
            for (int i = 0; i < Table.Length; i++)
            {
                Table[i] = new HashTable(b.ReadInt32(), b.ReadInt32());
            }
        }
        public Hsht(int m, int count)
        {
            Magic = m;
            Size = 0;
            Table = new HashTable[count];
            for (int i = 0; i < Table.Length; i++)
            {
                Table[i] = new HashTable();
            }
        }

        public void Write(LngBinaryWriter writer)
        {
            writer.Write(Magic);
            Size = Table.Length * 8;
            writer.Write(Size);

            int bucket = 0;
            for (int hash = 0; hash < Table.Length; hash++)
            {
                if (Table[hash].Count == 0)
                    writer.Write(0);
                else
                {
                    writer.Write(bucket);
                    bucket += Table[hash].Count;
                }
                writer.Write(Table[hash].Count);
            }
        }

        public IEnumerator<HashTable> GetEnumerator()
        {
            foreach (HashTable table in Table)
            {
                yield return table;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public IEnumerable<string> Keys
        {
            get
            {
                for (int hash = 0; hash < Table.Length; hash++)
                {
                    for (int i = 0; i < Table[hash].Count; i++)
                    {
                        yield return Table[hash][i].Key;
                    }
                }
            }
        }
        public IEnumerable<string> Values
        {
            get
            {
                for (int hash = 0; hash < Table.Length; hash++)
                {
                    for (int i = 0; i < Table[hash].Count; i++)
                    {
                        yield return Table[hash][i].Value;
                    }
                }
            }
        }
        public IEnumerable<HashEntry> Entries
        {
            get
            {
                for (int hash = 0; hash < Table.Length; hash++)
                {
                    for (int i = 0; i < Table[hash].Count; i++)
                    {
                        yield return Table[hash][i];
                    }
                }
            }
        }
    }
}
