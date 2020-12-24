namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class CsvFile : PerformanceFile
    {
        private string[] lines;

        public CsvFile(string _fileName, System.IO.Stream fileStream, CtfEditorGamePage _page)
            : base(_fileName, fileStream, _page)
        {
            using (CsvStreamReader reader = new CsvStreamReader(fileStream))
            {
                lines = reader.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                string[] values = lines[parentPage.lineIndex].Split(',');
                foreach (CtfEntryInfo entryInfo in parentPage.ctfEntryInfo)
                {
                    try
                    {
                        entry.Add(entryInfo.id, reader.ReadEntryData(entryInfo.type, values[entryInfo.id], entryInfo.name));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + Environment.NewLine +
                            entryInfo.id + " " + entryInfo.name + " " + values[entryInfo.id]);
                    }
                }

                parentPage.files.Add(this);
            }
        }

        public override void Write(string _fileName, System.IO.FileStream fileStream)
        {
            using (CsvStreamWriter writer = new CsvStreamWriter(fileStream))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (i == parentPage.lineIndex)
                    {
                        foreach (KeyValuePair<int, object> e in entry)
                        {
                            try
                            {
                                writer.WriteEntryData(parentPage.ctfEntryInfo[e.Key].type, e.Value);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message + Environment.NewLine +
                                    parentPage.ctfEntryInfo[e.Key].id + " " + parentPage.ctfEntryInfo[e.Key].name);
                            }
                        }
                    }
                    else
                    {
                        writer.Write(lines[i]);
                    }
                    if (i < lines.Length - 1)
                    {
                        writer.Write(Environment.NewLine); //\r\n
                    }
                }
            }

            base.Write(_fileName, fileStream);
        }
    }
}
