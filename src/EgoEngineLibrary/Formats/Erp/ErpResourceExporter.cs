using EgoEngineLibrary.Archive.Erp;
using System;
using System.IO;

namespace EgoEngineLibrary.Formats.Erp
{
    public class ErpResourceExporter
    {
        private const string QEscString = "%3F";
        private const string FragNameDelim = "!!!";
        private static readonly int FragNameDelimLength = 4 + FragNameDelim.Length; // frag name always 4 chars

        public void Export(ErpFile erp, string folderPath,
                           IProgress<string>? progressStatus, IProgress<int>? progressPercentage,
                           string filter="")
        {
            var success = 0;
            var fail = 0;

            for (var i = 0; i < erp.Resources.Count;)
            {
                var resource = erp.Resources[i];
                progressStatus?.Report("Exporting " + Path.Combine(resource.Folder, resource.FileName) + "... ");

                try
                {
                    if (string.IsNullOrWhiteSpace(filter) || resource.FileName.EndsWith(filter))
                    {
                        ExportResource(resource, folderPath);
                        progressStatus?.Report("SUCCESS" + Environment.NewLine);
                        ++success;
                    }
                }
                catch
                {
                    progressStatus?.Report("FAIL" + Environment.NewLine);
                    ++fail;
                }

                ++i;
                progressPercentage?.Report(i);
            }

            progressStatus?.Report(string.Format("{0} Succeeded, {1} Failed", success, fail));
        }

        public void ExportResource(ErpResource resource, string folderPath)
        {
            var outputDir = Path.Combine(folderPath, resource.Folder);
            Directory.CreateDirectory(outputDir);

            for (var i = 0; i < resource.Fragments.Count; ++i)
            {
                var fragment = resource.Fragments[i];
                var fileName = GetFragmentFileName(resource, fragment, i);
                var filePath = Path.Combine(outputDir, fileName);

                using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                using var decompressStream = fragment.GetDecompressDataStream(true);
                decompressStream.CopyTo(fs);
            }
        }

        public static string GetFragmentFileName(ErpResource resource, ErpFragment fragment)
        {
            var fragmentIndex = resource.Fragments.IndexOf(fragment);
            if (fragmentIndex < 0)
            {
                throw new InvalidOperationException("The fragment does not belong to this resource.");
            }

            return GetFragmentFileName(resource, fragment, fragmentIndex);
        }
        private static string GetFragmentFileName(ErpResource resource, ErpFragment fragment, int fragmentIndex)
        {
            var name = resource.FileName;
            name = name.Replace("?", QEscString);
            name = Path.GetFileNameWithoutExtension(name) + FragNameDelim
                + fragment.Name + fragmentIndex.ToString("000")
                + Path.GetExtension(name);
            return name;
        }

        public void Import(ErpFile erp, string[] files, IProgress<string>? progressStatus, IProgress<int>? progressPercentage)
        {
            var success = 0;
            var fail = 0;
            var skip = 0;

            for (var i = 0; i < erp.Resources.Count;)
            {
                var resource = erp.Resources[i];
                progressStatus?.Report("Importing " + Path.Combine(resource.Folder, resource.FileName) + "... ");

                try
                {
                    if (ImportResource(resource, files))
                    {
                        progressStatus?.Report("SUCCESS" + Environment.NewLine);
                        ++success;
                    }
                    else
                    {
                        progressStatus?.Report("SKIP" + Environment.NewLine);
                        ++skip;
                    }
                }
                catch
                {
                    progressStatus?.Report("FAIL" + Environment.NewLine);
                    ++fail;
                }

                ++i;
                progressPercentage?.Report(i);
            }

            progressStatus?.Report(string.Format("{0} Succeeded, {1} Skipped, {2} Failed", success, skip, fail));
        }

        public bool ImportResource(ErpResource resource, string[] files)
        {
            var fragmentsImported = 0;

            foreach (var f in files)
            {
                var extension = Path.GetExtension(f);
                var name = Path.GetFileNameWithoutExtension(f);
                var resTextIndex = name.LastIndexOf(FragNameDelim);
                if (resTextIndex == -1)
                {
                    continue;
                }

                var resIndex = int.Parse(name.Substring(resTextIndex + FragNameDelimLength, 3));
                name = Path.GetDirectoryName(f) + "\\" + (name.Remove(resTextIndex) + extension).Replace(QEscString, "?");
                if (name.EndsWith(Path.Combine(resource.Folder, resource.FileName), StringComparison.InvariantCultureIgnoreCase))
                {
                    var fragment = resource.Fragments[resIndex];

                    var data = File.ReadAllBytes(f);
                    fragment.SetData(data);
                    ++fragmentsImported;
                }
            }

            if (fragmentsImported == resource.Fragments.Count)
                return true;
            return false;
        }
    }
}
