using EgoEngineLibrary.Conversion;

namespace EgoEngineLibrary.Language
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class LngFile
    {
        public static Encoding encoding = Encoding.UTF8;
        public int magic;
        public Hshs hshs;
        public Hsht hsht;
        public SidA sida;
        public SidB sidb;
        public LngB lngb;
        public bool hasChanges;
        private int count;
        public int Count
        {
            get { return count; }
        }

        public LngFile(Stream fileStream)
        {
            using (LngBinaryReader b = new LngBinaryReader(EndianBitConverter.Big, fileStream))
            {
                byte[] keys, values;
                hasChanges = false;
                magic = b.ReadInt32();
                // SIZE - Skip
                b.ReadInt32();
                // Hash Section (HSHS)
                hshs = new Hshs(b);
                // Hash Table (HSHT)
                hsht = new Hsht(b);
                // Offsets (SIDA)
                sida = new SidA(b);
                // Keys (SIDB)
                sidb = new SidB(b);
                keys = b.ReadBytes(sidb.Size);
                // Values (LNGB)
                lngb = new LngB(b);
                values = b.ReadBytes(lngb.Size);
                count = sida.Entry.Length;
                for (int i = 0; i < sida.Entry.Length; i++)
                {
                    HashEntry hEntry = new HashEntry();
                    hEntry.Key = ReadTerminatedString(keys, sida.Entry[i].KeyOffset, 0x00);
                    hEntry.Value = ReadTerminatedString(values, sida.Entry[i].ValueOffset, 0x00);
                    hsht.Table[GetHash(hEntry.Key)].Add(hEntry);
                }
            }
        }
        public LngFile(DataSet data)
        {
            hasChanges = false;
            DataTable DT = data.Tables["info"] ??
                throw new InvalidDataException("The dataset does not have a table named info.");
            magic = (int)DT.Rows[0][0];
            count = (int)DT.Rows[0][1];
            hshs = new Hshs((int)DT.Rows[0][2], (uint)DT.Rows[0][3], (uint)DT.Rows[0][4], (uint)DT.Rows[0][5]);
            hsht = new Hsht((int)DT.Rows[0][6], (int)hshs.Buckets);
            sida = new SidA((int)DT.Rows[0][7]);
            sidb = new SidB((int)DT.Rows[0][8]);
            lngb = new LngB((int)DT.Rows[0][9]);

            DT = data.Tables["entry"] ??
                throw new InvalidDataException("The dataset does not have a table named entry.");
            count = DT.Rows.Count;
            resize();
            count = 0;
            foreach (DataRow row in DT.Rows)
            {
                Add((string)row[0], (string)row[1], false);
            }
        }
        public LngFile(DataTable DT)
        {
            hasChanges = false;
            magic = (int)DT.Rows[0][0];
            count = (int)DT.Rows[0][1];
            hshs = new Hshs((int)DT.Rows[0][2], (uint)DT.Rows[0][3], (uint)DT.Rows[0][4], (uint)DT.Rows[0][5]);
            hsht = new Hsht((int)DT.Rows[0][6], (int)hshs.Buckets);
            sida = new SidA((int)DT.Rows[0][7]);
            sidb = new SidB((int)DT.Rows[0][8]);
            lngb = new LngB((int)DT.Rows[0][9]);
        }

        public void Write(Stream fileStream)
        {
            // SETUP
            sida.Size = count * 8;
            sida.Entry = new SidaEntry[count];
            List<byte> keys = new List<byte>(sidb.Size), values = new List<byte>(lngb.Size);
            byte[] keyBytes, valueBytes;
            int keyOffset = 0, valueOffset = 0;
            int index = 0;
            foreach (HashEntry entry in hsht.Entries)
            {
                sida.Entry[index].KeyOffset = keyOffset;
                sida.Entry[index].ValueOffset = valueOffset;

                keyBytes = encoding.GetBytes(entry.Key);
                valueBytes = encoding.GetBytes(entry.Value);
                keys.AddRange(keyBytes);
                keys.Add(0x00);
                values.AddRange(valueBytes);
                values.Add(0x00);

                keyOffset += keyBytes.Length + 1;
                valueOffset += valueBytes.Length + 1;
                index++;
            }
            sidb.Size = keyOffset;
            lngb.Size = valueOffset;

            // Write
            using (LngBinaryWriter b = new LngBinaryWriter(BigEndianBitConverter.Big, fileStream))
            {
                b.Write(magic);
                // Final size = all section sizes + hshsSize12 + 5 * 4byte magics + 5 * 4byte sizes + sidanumEntries + lngtmagic + lngsize
                b.Write(0);

                hshs.Write(b);
                hsht.Write(b);

                sida.Write(b);
                // SIDB
                b.Write(sidb.Magic);
                b.Write(sidb.Size);
                b.Write(keys.ToArray());
                // LNGB
                b.Write(lngb.Magic);
                b.Write(lngb.Size);
                b.Write(values.ToArray());

                b.Seek(4, SeekOrigin.Begin);
                b.Write((int)b.BaseStream.Length);
            }

            hasChanges = false;
        }
        public DataSet WriteXml(Stream fileStream, DataTable? dataSource = null)
        {
            DataSet LNG = new DataSet("language");
            LNG.Tables.Add(GetInfoTable());
            if (dataSource == null)
                LNG.Tables.Add(GetDataTable());
            else
                LNG.Tables.Add(dataSource);
            LNG.WriteXml(fileStream, XmlWriteMode.WriteSchema);
            //LNG.WriteXmlSchema(Path.GetFullPath(fileName).Replace(Path.GetFileName(fileName), string.Empty) +
            //Path.GetFileNameWithoutExtension(fileName) + "_schema.xsd");
            hasChanges = false;
            return LNG;
        }

        public DataTable GetInfoTable()
        {
            DataTable DT = new DataTable("info");
            DT.Columns.Add("lngMagic", typeof(int));
            DT.Columns.Add("lngCount", typeof(int));
            DT.Columns.Add("hshsMagic", typeof(int));
            DT.Columns.Add("hshsBuckets", typeof(uint));
            DT.Columns.Add("hshsSeed", typeof(uint));
            DT.Columns.Add("hshsMultiplier", typeof(uint)); // 5

            DT.Columns.Add("hshtMagic", typeof(int));
            DT.Columns.Add("sidaMagic", typeof(int));
            DT.Columns.Add("sidbMagic", typeof(int));
            DT.Columns.Add("lngbMagic", typeof(int));
            DataRow row = DT.NewRow();
            row[0] = magic;
            row[1] = count;
            row[2] = hshs.Magic;
            row[3] = hshs.Buckets;
            row[4] = hshs.Seed;
            row[5] = hshs.Multiplier;
            row[6] = hsht.Magic;
            row[7] = sida.Magic;
            row[8] = sidb.Magic;
            row[9] = lngb.Magic;
            DT.Rows.Add(row);
            return DT;
        }
        public DataTable GetDataTable()
        {
            DataTable DT = new DataTable("entry");
            DT.CaseSensitive = true;
            DT.Columns.Add("LNG_Key", typeof(string));
            DT.Columns.Add("LNG_Value", typeof(string));
            DT.PrimaryKey = new DataColumn[] { DT.Columns["LNG_Key"]! };
            DataRow row;
            foreach (HashEntry entry in hsht.Entries)
            {
                row = DT.NewRow();
                row[0] = entry.Key;
                row[1] = entry.Value;
                try
                {
                    DT.Rows.Add(row);
                }
                catch (ConstraintException) { }//MessageBox.Show(sidbItems[i]); }
            }
            return DT;
        }

        public void Add(string key, string value, bool shouldResize = true)
        {
            int entryCount;
            uint hash = GetHash(key);

            entryCount = hsht.Table[hash].Count;

            if (entryCount == 0)
            {
                hsht.Table[hash].Add(new HashEntry(key, value));
                count++;
            }
            else
            {
                int i = 0;
                while (true)
                {
                    if (hsht.Table[hash][i].Key == key)
                    {
                        hsht.Table[hash][i].Value = value;
                        break;
                    }
                    i++;
                    if (i >= entryCount)
                    {
                        hsht.Table[hash].Add(new HashEntry(key, value));
                        count++;
                        break;
                    }
                }
            }

            if (shouldResize)
                resize();
        }
        public void Remove(string key, bool shouldResize = true)
        {
            int entryCount;
            uint hash = GetHash(key);

            entryCount = hsht.Table[hash].Count;

            if (entryCount == 0)
                return;
            else
            {
                int i = 0;
                while (true)
                {
                    if (hsht.Table[hash][i].Key == key)
                    {
                        hsht.Table[hash].RemoveAt(i);
                        count--;
                        break;
                    }
                    i++;
                    if (i >= entryCount)
                    {
                        return;
                    }
                }
            }

            if (shouldResize)
                resize();
        }
        private void resize()
        {
            int newTableSize = count / 2 + 1;
            if (newTableSize == hshs.Buckets)
                return;
            hshs.Buckets = (uint)newTableSize;

            HashTable[] oldTable = hsht.Table;
            hsht.Table = new HashTable[hshs.Buckets];
            count = 0;

            for (int i = 0; i < hshs.Buckets; i++)
            {
                hsht.Table[i] = new HashTable();
            }

            for (int i = 0; i < oldTable.Length; i++)
            {
                for (int j = 0; j < oldTable[i].Count; j++)
                {
                    Add(oldTable[i][j].Key, oldTable[i][j].Value, false);
                }
            }
        }

        private uint GetHash(string key)
        {
            char[] keyChar = key.ToCharArray();
            uint i = hshs.Seed;
            for (int j = 0; j < keyChar.Length; j++)
            {
                i = (i * hshs.Multiplier) + (uint)keyChar[j];
            }
            return (i % hshs.Buckets);
        }
        public string this[string key]
        {
            get
            {
                uint hash;
                int index = lookup(key, out hash);
                return index == -1 ? string.Empty : hsht.Table[hash][index].Value;
            }
        }
        public bool ContainsKey(string key)
        {
            return lookup(key) == -1 ? false : true;
        }
        private int lookup(string key, out uint hash)
        {
            int entryCount;
            hash = GetHash(key);

            entryCount = hsht.Table[hash].Count;

            if (entryCount == 0) return -1;
            int i = 0;
            while (true)
            {
                if (hsht.Table[hash][i].Key == key)
                {
                    break;
                }
                i++;
                if (i >= entryCount)
                {
                    return -1;
                }
            }
            return i;
        }
        private int lookup(string key)
        {
            uint x;
            return lookup(key, out x);
        }

        private string ReadTerminatedString(byte[] data, int offset, byte terminator)
        {
            List<byte> bytes = new List<byte>();
            byte b = data[offset];
            while (b != terminator)
            {
                bytes.Add(b);
                offset++;
                b = data[offset];
            }
            return encoding.GetString(bytes.ToArray());
        }

        public LngFile GetDifferences(LngFile two)
        {
            // Setup Differences Language
            DataSet differencesSet = new DataSet("language");
            differencesSet.Tables.Add(GetInfoTable());
            // Copy Structure
            DataTable table = GetDataTable();
            DataTable tableTwo = two.GetDataTable();
            DataTable diffs = table.Clone();

            // Get Differences and Load them into DataTable diffs
            for (int i = 0; i < tableTwo.Rows.Count; i++)
            {
                DataRow? tableRow = table.Rows.Find(tableTwo.Rows[i][0]);
                if (tableRow == null || !tableRow[1].Equals(tableTwo.Rows[i][1]))
                {
                    diffs.ImportRow(tableTwo.Rows[i]);
                }
            }

            // Save differences into XML file
            differencesSet.Tables.Add(diffs);
            return new LngFile(differencesSet);
        }
        public void MergeDifferences(DataTable table, DataTable tableTwo)
        {
            foreach (DataRow rowTwo in tableTwo.Rows)
            {
                DataRow? tableRow = table.Rows.Find(rowTwo[0]);
                if (tableRow == null)
                {
                    table.ImportRow(rowTwo);
                    Add((string)rowTwo[0], (string)rowTwo[1], false);
                }
                else if (!tableRow[1].Equals(rowTwo[1]))
                {
                    tableRow[1] = rowTwo[1];
                    Add((string)rowTwo[0], (string)rowTwo[1], false);
                }
            }
            resize();
        }
    }
}
