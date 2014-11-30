namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Hshs
    {
        public int Magic;
        public int Size;
        public uint Buckets;
        public uint Seed;
        public uint Multiplier;

        public Hshs(LngBinaryReader b)
        {
            Magic = b.ReadInt32();
            Size = b.ReadInt32();
            Buckets = b.ReadUInt32();
            Seed = b.ReadUInt32();
            Multiplier = b.ReadUInt32();
        }
        public Hshs(int m, uint b, uint s, uint mult)
        {
            Magic = m;
            Size = 12;
            Buckets = b;
            Seed = s;
            Multiplier = mult;
        }

        public void Write(LngBinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(Size);
            writer.Write(Buckets);
            writer.Write(Seed);
            writer.Write(Multiplier);
        }
    }
}
