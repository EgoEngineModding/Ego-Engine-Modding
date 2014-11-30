namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class JpkFile
    {
        private int magic;

        public JpkFile(System.IO.Stream fileStream)
        {
            using (JpkBinaryReader reader = new JpkBinaryReader(EndianBitConverter.Little, fileStream))
            {
                magic = reader.ReadInt32();
            }
        }
    }
}
