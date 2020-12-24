using System;
namespace EgoEngineLibrary.Language
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class SidB
    {
        public int Magic;
        public int Size;

        public SidB(LngBinaryReader b)
        {
            Magic = b.ReadInt32();
            Size = b.ReadInt32();
        }
        public SidB(int m)
        {
            Magic = m;
        }
    }
}
