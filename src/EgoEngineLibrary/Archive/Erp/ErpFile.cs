using MiscUtil.Conversion;
using System;
using System.Collections.Generic;
using System.IO;

namespace EgoEngineLibrary.Archive.Erp
{
    public class ErpFile
    {
        public int Version { get; set; }

        public ulong ResourceOffset { get; set; }

        public List<ErpResource> Resources { get; set; }

        private ulong _resourceInfoTotalLength;

        public ErpFile()
        {
            Version = 4;
            Resources = new List<ErpResource>();
        }

        public void Read(Stream stream)
        {
            using var reader = new ErpBinaryReader(EndianBitConverter.Little, stream);
            var magic = reader.ReadUInt32();
            if (magic != 1263555141)
            {
                throw new Exception("This is not an ERP file!");
            }

            Version = reader.ReadInt32();
            reader.ReadBytes(8); // padding
            reader.ReadBytes(8); // info offset
            reader.ReadBytes(8); // info size

            ResourceOffset = reader.ReadUInt64();
            reader.ReadBytes(8); // padding

            var numFiles = reader.ReadInt32();
            var numTempFile = reader.ReadInt32();

            for (var i = 0; i < numFiles; ++i)
            {
                var entry = new ErpResource(this);
                entry.Read(reader);
                Resources.Add(entry);
            }
        }

        public void Write(Stream stream)
        {
            using var writer = new ErpBinaryWriter(EndianBitConverter.Little, stream);
            var numTempFiles = UpdateOffsets();

            writer.Write(1263555141);

            writer.Write(Version);
            writer.Write(0L);
            writer.Write(48L);
            writer.Write(_resourceInfoTotalLength);

            writer.Write(ResourceOffset);
            writer.Write(0L);

            writer.Write(Resources.Count);
            writer.Write(numTempFiles);

            foreach (var entry in Resources)
            {
                entry.Write(writer);
            }

            foreach (var entry in Resources)
            {
                foreach (var frag in entry.Fragments)
                {
                    //writer.Write((UInt16)0xDA78);
                    writer.Write(frag.GetDataArray(false));
                }
            }
        }

        public int UpdateOffsets()
        {
            ulong resourceDataOffset = 0;
            var numTempFiles = 0;

            _resourceInfoTotalLength = (ulong)Resources.Count * 4 + 8;
            foreach (var entry in Resources)
            {
                _resourceInfoTotalLength += entry.UpdateOffsets();

                foreach (var frag in entry.Fragments)
                {
                    ++numTempFiles;
                    frag.Offset = resourceDataOffset;
                    resourceDataOffset += frag.PackedSize;
                }
            }

            ResourceOffset = 48 + _resourceInfoTotalLength;
            return numTempFiles;
        }

        public ErpResource FindResource(string identifier)
        {
            var res = Resources.Find(x => x.Identifier == identifier);

            if (res == null)
            {
                throw new InvalidOperationException($"Could not find resource: {identifier}");
            }

            return res;
        }
    }
}
