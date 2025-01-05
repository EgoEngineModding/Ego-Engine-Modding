using EgoEngineLibrary.Conversion;

namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CtfFile : PerformanceFile
    {
        public CtfFile(string _fileName, System.IO.Stream fileStream, CtfEditorGamePage _page)
            : base(_fileName, fileStream, _page)
        {
            int flag = -1;

            using (CtfBinaryReader reader = new CtfBinaryReader(EndianBitConverter.Little, fileStream))
            {
                foreach (CtfEntryInfo entryInfo in parentPage.ctfEntryInfo)
                {
                    try
                    {
                        if (entryInfo.name == "magic")
                        {
                            entry.Add(entryInfo.id, reader.ReadInt32());
                        }
                        else if (entryInfo.name == "flag")
                        {
                            entry.Add(entryInfo.id, reader.ReadInt32());
                            flag = (int)entry[entryInfo.id];
                        }
                        else if (entryInfo.IsUsed(flag))
                        {
                            if (entryInfo.linkID != -1)
                            {
                                CtfEntryInfo linkedEntryInfo = parentPage.ctfEntryInfo.First(x => x.refID == entryInfo.linkID);
                                if (Convert.ToBoolean(entry[linkedEntryInfo.id]))
                                {
                                    entry.Add(entryInfo.id, reader.ReadEntryData(entryInfo.type));
                                }
                            }
                            else
                            {
                                entry.Add(entryInfo.id, reader.ReadEntryData(entryInfo.type));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + Environment.NewLine + entryInfo.id + " " + entryInfo.name);
                    }
                }
                CtfEntryInfo last = parentPage.ctfEntryInfo.Last(x => x.minFlag == 0);
                if (!entry.ContainsKey(last.id))
                {
                    throw new Exception("Reader did not read last required value");
                }
                // Use -1 For Dirt 3, hopefully won't cause problems with other games
                //System.Windows.Forms.MessageBox.Show(reader.BaseStream.Position + " " + reader.BaseStream.Length);
                if (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                {
                    throw new Exception("Reader did not reach end of file!");
                }

                parentPage.files.Add(this);
            }
        }

        public override void Write(string _fileName, System.IO.FileStream fileStream)
        {
            using (CtfBinaryWriter writer = new CtfBinaryWriter(new LittleEndianBitConverter(), fileStream))
            {
                foreach (KeyValuePair<int, object> e in entry)
                {
                    try
                    {
                        if (Convert.ChangeType(e.Value, parentPage.ctfEntryInfo[e.Key].realType) == null)
                        {
                            writer.WriteEntryData(parentPage.ctfEntryInfo[e.Key].type, parentPage.ctfEntryInfo[e.Key].defaultValue);
                        }
                        else
                        {
                            writer.WriteEntryData(parentPage.ctfEntryInfo[e.Key].type, e.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + Environment.NewLine +
                            parentPage.ctfEntryInfo[e.Key].id + " " + parentPage.ctfEntryInfo[e.Key].name);
                    }
                }
            }

            base.Write(_fileName, fileStream);
        }
    }
}
