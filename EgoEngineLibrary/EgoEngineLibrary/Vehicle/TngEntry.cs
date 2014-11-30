namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    public class TngEntry
    {
        public Int16 Id;
        public int InfoId;
        public Dictionary<string, object> Data;

        public TngEntry[] ChildEntry;
        public bool HasChildren
        {
            get { return ChildEntry.Length > 0; }
        }

        public TngFile File;
        public string Name
        {
            get
            {
                if (File.TngInfo.ContainsKey(InfoId))
                    return File.TngInfo[InfoId].Name;
                else
                    return "NameNotFound";
            }
        }

        public TngEntry(TngBinaryReader reader, TngFile file, bool isParent = true)
        {
            File = file;
            InfoId = reader.ReadInt32();
            Data = new Dictionary<string, object>();

            if (isParent)
            {
                Id = reader.ReadInt16();
                Data.Add("InstructionCode", reader.ReadInt32());
                switch ((int)Data["InstructionCode"])
                {
                    case 0:
                        Data.Add("Val", reader.ReadSingle());
                        break;
                    case 1:
                        Data.Add("LinkedInfoId", reader.ReadInt32());
                        Data.Add("Val", reader.ReadSingle());
                        break;
                    default:
                        throw new Exception("Invalid instruction code!");
                }

                Data.Add("InstructionCode2", reader.ReadInt32());
                switch ((int)Data["InstructionCode2"])
                {
                    case 4:
                    case 1:
                    case 0:
                        Data.Add("Val2", reader.ReadSingle());
                        Data.Add("Val3", reader.ReadSingle());
                        break;
                    case 3:
                        Data.Add("LinkedInfoId2", reader.ReadInt32());
                        break;
                    default:
                        throw new Exception("Invalid instruction code 2! " + reader.BaseStream.Position);
                }

                ChildEntry = new TngEntry[reader.ReadInt32()];
                for (int i = 0; i < ChildEntry.Length; i++)
                {
                    ChildEntry[i] = new TngEntry(reader, file, false);
                }
            }
            else
            {
                Id = -1;
                Data.Add("Val", reader.ReadSingle());
                Data.Add("Val2", reader.ReadSingle());
                Data.Add("Num", reader.ReadInt32());
                ChildEntry = new TngEntry[0];
            }
        }
        public void Write(TngBinaryWriter writer)
        {
            writer.Write(InfoId);

            if (Id != -1)
            {
                writer.Write(Id);

                foreach (KeyValuePair<string, object> data in Data)
                {
                    writer.WriteObject(data.Value);
                }

                writer.Write(ChildEntry.Length);
                for (int i = 0; i < ChildEntry.Length; i++)
                {
                    ChildEntry[i].Write(writer);
                }
            }
            else
            {
                foreach (KeyValuePair<string, object> data in Data)
                {
                    writer.WriteObject(data.Value);
                }
            }
        }

        public void CreateDgvTable(DataGridView dgv)
        {
            CreateDgvRow(dgv, "InfoId", InfoId, true);

            foreach (KeyValuePair<string, object> pair in Data)
            {
                bool readO = pair.Key.Contains("InstructionCode");
                CreateDgvRow(dgv, pair.Key, pair.Value, readO);
                if (pair.Key.Contains("LinkedInfoId") && File.TngInfo.ContainsKey((int)pair.Value))
                {
                    CreateDgvRow(dgv, pair.Key + "Name", File.TngInfo[(int)pair.Value].Name, true);
                }
            }
        }
        private void CreateDgvRow(DataGridView dgv, string name, object data, bool readOnly = false)
        {
            DataGridViewRow row = dgv.Rows[dgv.Rows.Add(data)];
            row.Cells[0].ValueType = data.GetType();
            row.HeaderCell.Value = name;
            row.ReadOnly = readOnly;
        }
    }
}
