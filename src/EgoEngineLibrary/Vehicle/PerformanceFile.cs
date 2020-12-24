namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class PerformanceFile
    {
        public string name;
        public string fileName;
        public CtfEditorGamePage parentPage;
        public bool hasChanges;
        public SortedDictionary<int, object> entry = new SortedDictionary<int, object>();

        protected PerformanceFile(string _fileName, System.IO.Stream fileStream, CtfEditorGamePage _page)
        {
            name = System.IO.Path.GetFileName(_fileName);
            fileName = _fileName;
            parentPage = _page;
            hasChanges = false;
        }

        public virtual void Write(string _fileName, System.IO.FileStream fileStream)
        {
            name = System.IO.Path.GetFileName(_fileName);
            fileName = _fileName;
            hasChanges = false;
        }
    }
}
