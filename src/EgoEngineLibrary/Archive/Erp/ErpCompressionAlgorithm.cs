using System;
using System.Collections.Generic;
using System.Text;

namespace EgoEngineLibrary.Archive.Erp
{
    public enum ErpCompressionAlgorithm : byte
    {
        None,
        Zlib = 0x01,
        
        /// <summary>
        /// This is only use for mip compression.
        /// TODO: Split out mip compression algorithms into new enum.
        /// </summary>
        LZ4 = 0x03,
        
        /// <summary>
        /// This was introduced in Grid Legends.
        /// </summary>
        ZStandard2 = 0x03,
        
        ZStandard = 0x10,
        None2 = 0x81,
        None3 = 0x90
    }
}
