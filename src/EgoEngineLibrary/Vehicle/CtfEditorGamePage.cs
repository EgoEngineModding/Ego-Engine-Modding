namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    public enum CtfEditorMainTabs
    {
        Dirt, //dirtTabPage
        Grid, // gridTabPage
        Formula1, // f12012TabPage
        Other // grid2TabPage
        //Dirt2, // dirt2TabPage
        //Dirt3, // dirt3TabPage
        //DirtShowdown, // dirtShowdownTabPage
        //F12010, // f12010TabPage
        //F12011, // f12011TabPage
    }

    public enum CtfEditorFilterIndex
    {
        CTF = 1, CSV, ALL
    }

    public class CtfEditorGamePage
    {
        public CtfEditorMainTabs parentTab;
        private CtfEditorFilterIndex filterIndex;
        public CtfEditorFilterIndex FilterIndex
        {
            get { return filterIndex; }
        }
        public int lineIndex;
        public CtfEntryInfo[] ctfEntryInfo;
        public List<PerformanceFile> files;

        public CtfEditorGamePage(System.IO.Stream fileStream, CtfEditorMainTabs _parentTab)
        {
            using (fileStream)
            {
                XmlDocument ctfSchema = new XmlDocument();
                ctfSchema.Load(fileStream);

                if (ctfSchema.DocumentElement is null)
                    throw new InvalidDataException("The ctf schema does not have a root element.");

                filterIndex = GetFilterIndex(ctfSchema.DocumentElement.GetAttribute("extension"));
                Int32.TryParse(ctfSchema.DocumentElement.GetAttribute("line"), out lineIndex);
                parentTab = _parentTab;

                ctfEntryInfo = new CtfEntryInfo[ctfSchema.DocumentElement.ChildNodes.Count];
                int i = 0;
                foreach (XmlElement entry in ctfSchema.DocumentElement.ChildNodes)
                {
                    ctfEntryInfo[i] = new CtfEntryInfo(i, entry);
                    i++;
                }

                files = new List<PerformanceFile>();
            }
        }
        private CtfEditorFilterIndex GetFilterIndex(string extension)
        {
            switch (extension)
            {
                case "ctf":
                    return CtfEditorFilterIndex.CTF;
                case "csv":
                    return CtfEditorFilterIndex.CSV;
                case "all":
                    return CtfEditorFilterIndex.ALL;
                default:
                    return CtfEditorFilterIndex.CTF;
            }
        }

        public bool ContainsFile(string fileName)
        {
            foreach (PerformanceFile file in files)
            {
                if (file.fileName == fileName)
                    return true;
            }
            return false;
        }
    }
}
