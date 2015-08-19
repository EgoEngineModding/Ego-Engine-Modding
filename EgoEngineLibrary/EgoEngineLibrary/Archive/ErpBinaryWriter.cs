namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using MiscUtil.IO;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class ErpBinaryWriter : EndianBinaryWriter
    {
        public ErpBinaryWriter(EndianBitConverter bitConverter, System.IO.Stream stream)
            : base(bitConverter, stream)
        {
        }
    }
}
