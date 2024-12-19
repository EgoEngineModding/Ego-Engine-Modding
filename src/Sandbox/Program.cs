﻿using EgoEngineLibrary.Xml;

namespace Sandbox;

internal class Program
{
    static void Main(string[] args)
    {
        CQuadTreeSandbox.Run(args);
    }

    private static void XmlSandbox()
    {
        var files = Utils.GetFiles(@"C:\Games\Steam\steamapps\common\F1 2012");
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

                if (xml.Type != XmlType.Text)
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
                if (type != XmlType.Text)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
