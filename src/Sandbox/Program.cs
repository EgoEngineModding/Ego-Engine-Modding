using EgoEngineLibrary.Xml;

namespace Sandbox;

internal class Program
{
    static void Main(string[] args)
    {
        XmlSandbox();
    }

    private static void XmlSandbox()
    {
        var files = GetFiles(@"C:\Games\Steam\steamapps\common\F1 2012");
        foreach (var f in files)
        {
            try
            {
                using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var isXmlFile = XmlFile.IsXmlFile(fs);
                // if (f.EndsWith("reflections.xml"))
                // {
                //     int a = 55;
                // }
                Console.WriteLine($"{f} {isXmlFile}");
                var xml = new XmlFile(fs);
                //Console.WriteLine(xml.type);

                if (xml.type != XMLType.Text)
                {
                    using var ms = new MemoryStream();
                    xml.Write(ms);

                    var mso = new MemoryStream();
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.CopyTo(mso);
                    mso.Seek(0, SeekOrigin.Begin);
                    ms.Seek(0, SeekOrigin.Begin);
                    var orig = mso.ToArray();
                    var writ = ms.ToArray();
                    if (!orig.SequenceEqual(writ))
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            catch (Exception e)
            {
                using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var type = XmlFile.GetXmlType(fs);
                if (type != XMLType.Text)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        static IEnumerable<string> GetFiles(params string[] gameFolders)
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".xml" };
            var files = Enumerable.Empty<string>();
            foreach (var folder in gameFolders)
            {
                var folderFiles = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                    .Where(x => extensions.Contains(Path.GetExtension(x)));
                files = files.Concat(folderFiles);
            }

            return files;
        }
    }
}