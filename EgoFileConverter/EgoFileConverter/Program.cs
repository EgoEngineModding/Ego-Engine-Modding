using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Console.WriteLine("Drag and drop a a file on the EXE to convert.");
                }

                foreach (string f in args)
                {
                    try
                    {
                        Console.WriteLine("Processing " + Path.GetFileName(f) + "...");

                        if (Path.GetExtension(f) == ".xml")
                        {
                            convert(f, f + ".xml");
                        }
                        else if (Path.GetExtension(f) == ".json")
                        {
                            PkgFile file = PkgFile.ReadJson(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                            file.WritePkg(File.Open(f + ".pkg", FileMode.Create, FileAccess.Write, FileShare.Read));
                            Console.WriteLine("Success! Pkg created.");
                        }
                        else
                        {

                            PkgFile file = PkgFile.ReadPkg(File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read));
                            file.WriteJson(File.Open(f + ".json", FileMode.Create, FileAccess.Write, FileShare.Read));
                            Console.WriteLine("Success! Json created.");
                            continue;
                            Console.WriteLine("Invalid file extension!");
                            Console.WriteLine("Drag and drop a pkg or json file on the EXE to convert.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to convert the file!");
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }
        }


        private static void convert(string path, string path2, int conversionType = 0)
        {
            // Get ConvertType
            XMLType convertType = (XMLType)conversionType;
            if (!Enum.IsDefined(typeof(XMLType), conversionType))
            {
                Console.WriteLine("ERROR: Could not figure out the conversion type!");
                return;
            }

            // Load File
            XmlFile file = new XmlFile(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            Console.WriteLine("INFO: Converting file {0} from {1} to {2} format.", Path.GetFileName(path), file.type, convertType);

            // Make sure File Type and Conversion Type are different
            if (file.type == convertType)
            {
                while (true)
                {
                    Console.WriteLine("WARNING: Invalid conversion type entered because the file is already in this format!");
                    Console.WriteLine("0 -- Text, 1 -- Bin Xml, 2 -- BXML Big, 3 -- BXML Little");
                    Console.WriteLine("Please enter the conversion type (excluding {0}), or type 'Exit' to quit: ", (int)file.type);

                    string cType = Console.ReadLine();
                    if (cType == "Exit")
                    {
                        return;
                    }

                    if (Int32.TryParse(cType, out conversionType) &&
                        conversionType != (int)file.type &&
                        Enum.IsDefined(typeof(XMLType), conversionType))
                    {
                        convertType = (XMLType)conversionType;
                        break;
                    }
                }
            }

            // Convert
            file.Write(File.Open(path2, FileMode.Create, FileAccess.Write, FileShare.Read), convertType);
            Console.WriteLine("Success!");
        }
    }
}
