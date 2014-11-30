namespace EgoEngineLibrary.Vehicle
{
    using MiscUtil.Conversion;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    public class TngFile
    {
        private int magic;
        private int entryCount;
        private int infoSize;
        public SortedDictionary<int, TngInfo> TngInfo;
        public TngEntry[] TngEntry;

        public TngFile(System.IO.Stream fileStream)
        {
            using (TngBinaryReader reader = new TngBinaryReader(EndianBitConverter.Little, fileStream))
            {
                magic = reader.ReadInt32();
                if (magic != 100)
                {
                    throw new Exception("This is not a tng file!");
                }
                entryCount = reader.ReadInt32();
                infoSize = reader.ReadInt32();
                long endInfoLocation = reader.BaseStream.Position + infoSize;

                TngInfo info;
                TngInfo = new SortedDictionary<int, TngInfo>();
                while (reader.BaseStream.Position < endInfoLocation)
                {
                    info = new TngInfo(reader, this);
                }

                TngEntry = new TngEntry[entryCount];
                TngEntry tngEntry;
                for (int i = 0; i < entryCount; i++)
                {
                    tngEntry = new TngEntry(reader, this);
                    TngEntry[tngEntry.Id] = tngEntry;
                }
            }
        }
        public void Write(System.IO.Stream fileStream)
        {
            using (TngBinaryWriter writer = new TngBinaryWriter(EndianBitConverter.Little, fileStream))
            {
                writer.Write(magic);
                writer.Write(TngEntry.Length);
                writer.Write(infoSize);

                foreach (KeyValuePair<int, TngInfo> tngInfo in TngInfo)
                {
                    writer.WriteTerminatedString(tngInfo.Value.Name, 0x00);
                }

                for (int i = 0; i < TngEntry.Length; i++)
                {
                    TngEntry[i].Write(writer);
                }
            }
        }

        public void CreateTreeViewList(TreeView tV)
        {
            foreach (TngEntry entry in TngEntry)
            {
                tV.Nodes.Add(CreateTreeViewNode(entry));
            }
        }
        public TreeNode CreateTreeViewNode(TngEntry entry)
        {
            TreeNode treeNode = new TreeNode();
            treeNode.Text = entry.Name;
            treeNode.Tag = entry;
            foreach (TngEntry subEntry in entry.ChildEntry)
            {
                treeNode.Nodes.Add(CreateTreeViewNode(subEntry));
            }
            return treeNode;
        }

        public void Add()
        {
            TngInfo tInfo = new TngInfo();
            tInfo.Id = infoSize;
            tInfo.Name = "gear_8th";
            tInfo.IsParent = false;
            infoSize += Encoding.UTF8.GetByteCount(tInfo.Name) + 1;
            TngInfo.Add(tInfo.Id, tInfo);

            tInfo = new TngInfo();
            tInfo.Id = infoSize;
            tInfo.Name = "F1/Transmission/Gear 8 Ratio Adjust";
            tInfo.IsParent = true;
            infoSize += Encoding.UTF8.GetByteCount(tInfo.Name) + 1;
            TngInfo.Add(tInfo.Id, tInfo);

            Array.Resize(ref TngEntry, entryCount + 1);
            TngEntry[entryCount] = TngEntry[27];
            entryCount++;
        }
    }
}
