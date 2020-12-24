namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class LngB
    {
        public int Magic;
        public int Size;

        public LngB(LngBinaryReader b)
        {
            Magic = b.ReadInt32();
            Size = b.ReadInt32();
        }
        public LngB(int m)
        {
            Magic = m;
        }
    }
}
