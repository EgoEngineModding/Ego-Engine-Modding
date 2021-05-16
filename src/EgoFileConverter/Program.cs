using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Language;
using EgoEngineLibrary.Xml;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Xml;

namespace EgoFileConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("--- " + Properties.Resources.AppTitleLong + " ---");
                if (args.Length == 0)
                {
                    Console.WriteLine("No input arguments were found!");
                    Console.WriteLine("Drag and drop one or more files on the EXE to convert.");
                }

                foreach (string f in args)
                {
                    try
                    {
                        Console.WriteLine("Processing " + Path.GetFileName(f) + "...");

                        Convert(f);
                    }
                    catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.WriteLine("Failed to convert the file!");
                        Console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }

        private static void Convert(string f)
        {
            string magic;
            string xmlMagic;
            using (var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PkgBinaryReader reader = new PkgBinaryReader(fs);
                magic = reader.ReadString(4);

                // Skip first byte since BXMLBig starts with \0 causing empty string
                reader.Seek(1, SeekOrigin.Begin);
                xmlMagic = reader.ReadString(3);
            }
            
            if (xmlMagic == "\"Rr" || xmlMagic == "BXM")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var fso = File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read);
                XmlFile file = new XmlFile(fsi);
                file.Write(fso, XMLType.Text);
                Console.WriteLine("Success! XML converted.");
            }
            else if (magic == "LNGT")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var fso = File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read);
                LngFile file = new LngFile(fsi);
                file.WriteXml(fso);
                Console.WriteLine("Success! Lng converted.");
            }
            else if (magic == "!pkg")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var fso = File.Open(f + ".json", FileMode.Create, FileAccess.Write, FileShare.Read);
                PkgFile file = PkgFile.ReadPkg(fsi);
                file.WriteJson(fso);
                Console.WriteLine("Success! Pkg converted.");
            }
            else
            {
                bool isJSON = false;
                JsonException jsonEx = null;
                try
                {
                    using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using var fso = File.Open(f + ".pkg", FileMode.Create, FileAccess.Write, FileShare.Read);
                    PkgFile pkgFile = PkgFile.ReadJson(fsi);
                    pkgFile.WritePkg(fso);
                    Console.WriteLine("Success! JSON converted.");
                    isJSON = true;
                }
                catch (JsonException e)
                {
                    jsonEx = e;
                }

                if (!isJSON)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    try
                    {
                        xmlDoc.Load(f);
                    }
                    catch (XmlException e)
                    {
                        throw new AggregateException("Could not determine the file type! Showing json, and xml errors: ", jsonEx, e);
                    }

                    if (xmlDoc.DocumentElement.Name == "language")
                    {
                        using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var fso = File.Open(f + ".lng", FileMode.Create, FileAccess.Write, FileShare.Read);
                        DataSet dataSet = new DataSet("language");
                        dataSet.ReadXml(fsi, XmlReadMode.ReadSchema);
                        LngFile file = new LngFile(dataSet);
                        file.Write(fso);
                        Console.WriteLine("Success! XML converted.");
                    }
                    else
                    {
                        using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                        using var fso = File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read);
                        XmlFile file = new XmlFile(fsi);
                        file.Write(fso);
                        Console.WriteLine("Success! XML converted.");
                    }
                }
            }
        }
    }
}
