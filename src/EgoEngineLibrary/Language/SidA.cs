namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SidA
    {
        public int Magic;
        public int Size;
        public SidaEntry[] Entry;

        public SidA(LngBinaryReader b)
        {
            Magic = b.ReadInt32();
            Size = b.ReadInt32();
            Entry = new SidaEntry[b.ReadInt32()];
            for (int i = 0; i < Entry.Length; i++)
            {
                Entry[i].KeyOffset = b.ReadInt32();
                Entry[i].ValueOffset = b.ReadInt32();
            }
        }
        public SidA(int m)
        {
            Magic = m;
            Entry = Array.Empty<SidaEntry>();
        }

        public void Write(LngBinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(Size);
            writer.Write(Entry.Length);
            for (int i = 0; i < Entry.Length; i++)
            {
                writer.Write(Entry[i].KeyOffset);
                writer.Write(Entry[i].ValueOffset);
            }
        }
    }
}
