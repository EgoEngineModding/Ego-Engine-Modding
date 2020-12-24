namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Bucket
    {
        public int count;
        public List<int[]> itemOffsets;

        public Bucket()
        {
            count = 0;
            itemOffsets = new List<int[]>();
        }
    }
}
