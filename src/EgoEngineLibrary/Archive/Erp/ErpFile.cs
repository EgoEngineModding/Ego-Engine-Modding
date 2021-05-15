namespace EgoEngineLibrary.Archive.Erp
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ErpFile
    {
        public int Version { get; set; }

        public ulong ResourceOffset { get; set; }

        public List<ErpResource> Resources { get; set; }

        private ulong _resourceInfoTotalLength;
        public Progress<int>? ProgressPercentage;
        public Progress<string>? ProgressStatus;

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

        public ErpResource FindResource(string fileName)
        {
            var res = TryFindResource(fileName);

            if (res == null)
            {
                throw new InvalidOperationException($"Could not find resource: {fileName}");
            }

            return res;
        }
        public ErpResource? TryFindResource(string fileName)
        {
            foreach (var entry in Resources)
            {
                if (entry.Identifier == fileName)
                {
                    return entry;
                }
            }

            return null;
        }

        public void Export(string folderPath)
        {
            var success = 0;
            var fail = 0;

            for (var i = 0; i < Resources.Count;)
            {
                (ProgressStatus as IProgress<string>)?.Report("Exporting " + Path.Combine(Resources[i].Folder, Resources[i].FileName) + "... ");

                try
                {
                    Resources[i].Export(folderPath);
                    (ProgressStatus as IProgress<string>)?.Report("SUCCESS" + Environment.NewLine);
                    ++success;
                }
                catch
                {
                    (ProgressStatus as IProgress<string>)?.Report("FAIL" + Environment.NewLine);
                    ++fail;
                }

                ++i;
                (ProgressPercentage as IProgress<int>)?.Report(i);
            }

            (ProgressStatus as IProgress<string>)?.Report(string.Format("{0} Succeeded, {1} Failed", success, fail));
        }

        public void Import(string[] files)
        {
            var success = 0;
            var fail = 0;
            var skip = 0;

            for (var i = 0; i < Resources.Count;)
            {
                (ProgressStatus as IProgress<string>)?.Report("Importing " + Path.Combine(Resources[i].Folder, Resources[i].FileName) + "... ");

                try
                {
                    if (Resources[i].Import(files))
                    {
                        (ProgressStatus as IProgress<string>)?.Report("SUCCESS" + Environment.NewLine);
                        ++success;
                    }
                    else
                    {
                        (ProgressStatus as IProgress<string>)?.Report("SKIP" + Environment.NewLine);
                        ++skip;
                    }
                }
                catch
                {
                    (ProgressStatus as IProgress<string>)?.Report("FAIL" + Environment.NewLine);
                    ++fail;
                }

                ++i;
                (ProgressPercentage as IProgress<int>)?.Report(i);
            }

            (ProgressStatus as IProgress<string>)?.Report(string.Format("{0} Succeeded, {1} Skipped, {2} Failed", success, skip, fail));
        }
    }
}
