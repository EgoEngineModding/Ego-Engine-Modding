using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Language;
using EgoEngineLibrary.Xml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    catch (Exception ex)
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
            using (FileStream fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PkgBinaryReader reader = new PkgBinaryReader(fs);
                magic = reader.ReadString(4);
                xmlMagic = magic.Substring(1);
            }
            
            if (xmlMagic == "\"Rr" || xmlMagic == "BXM")
            {
                XmlFile file = new XmlFile(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                file.Write(File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read), XMLType.Text);
                Console.WriteLine("Success! XML converted.");
            }
            else if (magic == "LNGT")
            {
                LngFile file = new LngFile(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                file.WriteXml(File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read));
                Console.WriteLine("Success! Lng converted.");
            }
            else if (magic == "!pkg")
            {
                PkgFile file = PkgFile.ReadPkg(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                file.WriteJson(File.Open(f + ".json", FileMode.Create, FileAccess.Write, FileShare.Read));
                Console.WriteLine("Success! Pkg converted.");
            }
            else
            {
                bool isJSON = false;
                JsonException jsonEx = null;
                try
                {
                    PkgFile pkgFile = PkgFile.ReadJson(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                    pkgFile.WritePkg(File.Open(f + ".pkg", FileMode.Create, FileAccess.Write, FileShare.Read));
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
                        DataSet dataSet = new DataSet("language");
                        dataSet.ReadXml(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read), XmlReadMode.ReadSchema);
                        LngFile file = new LngFile(dataSet);
                        file.Write(File.Open(f + ".lng", FileMode.Create, FileAccess.Write, FileShare.Read));
                        Console.WriteLine("Success! XML converted.");
                    }
                    else
                    {
                        XmlFile file = new XmlFile(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                        file.Write(File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read));
                        Console.WriteLine("Success! XML converted.");
                    }
                }
            }
        }
    }
}
