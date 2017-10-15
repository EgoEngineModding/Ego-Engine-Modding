using System;
using System.Collections.Generic;
using System.Text;

namespace EgoEngineLibrary.Archive.Erp
{
    public enum ErpCompressionAlgorithm : byte
    {
        None,
        Zlib,
        LZ4 = 3
    }
}
