namespace EgoEngineLibrary.Archive
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class ErpFile
    {
        private Stream _erpStream;
        public Stream ErpStream
        {
            get
            {
                if (_erpStream != null)
                {
                    return _erpStream;
                }

                throw new Exception("The erp stream is null!");
            }
        }
        public Int32 Version { get; set; }

        public UInt64 EntryOffset { get; set; }

        public List<ErpEntry> Entries { get; set; }

        public ErpFile()
        {
            this.Version = 3;
            this.Entries = new List<ErpEntry>();
        }

        public void Read(Stream stream)
        {
            ErpBinaryReader reader = new ErpBinaryReader(EndianBitConverter.Little, stream);
            uint magic = reader.ReadUInt32();
            if (magic != 1263555141)
            {
                throw new Exception("This is not an ERP file!");
            }

            this.Version = reader.ReadInt32();
            reader.ReadBytes(8); // padding
            reader.ReadBytes(8); // info offset
            reader.ReadBytes(8); // info size

            this.EntryOffset = reader.ReadUInt64();
            reader.ReadBytes(8); // padding

            Int32 numFiles = reader.ReadInt32();
            Int32 numTempFile = reader.ReadInt32();

            for (int i = 0; i < numFiles; ++i)
            {
                ErpEntry entry = new ErpEntry(this);
                entry.Read(reader);
                this.Entries.Add(entry);
            }

            this._erpStream = stream;
        }
    }
}
