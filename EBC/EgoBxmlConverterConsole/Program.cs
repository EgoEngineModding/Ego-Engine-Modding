using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EgoEngineLibrary.Xml;

namespace EgoBxmlConverter
{
    class Program
    {
        // X.x -- Use EgoEngineLibrary, Remove GUI from Solution
        static void Main(string[] args)
        {
            int conversionType = 0;
            //args = new string[] { @"C:\Games\Steam\steamapps\common\f1 2012 demo\database\schema.bin" };
            if (args.Length == 1)
            {
                // Drag/Drop Single File
                convert(args[0], Path.ChangeExtension(args[0], "C.xml"));
            }
            else if (args.Length == 4 && args[0] == "-c" && Int32.TryParse(args[1], out conversionType))
            {
                // Typical Command Line Conversion for Single File
                convert(args[2], args[3], conversionType);
            }
            else if (args.Length > 1 && File.Exists(args[0]) && File.Exists(args[1]))
            {
                // Drag/Drop for Multiple Files
                for (int i = 0; i < args.Length; i++)
                {
                    convert(args[i], Path.ChangeExtension(args[i], "C.xml"));
                }
            }
            else if (args.Length > 2 && args[0] == "-b" && Int32.TryParse(args[1], out conversionType))
            {
                // Command Line Batch Conversion to Single Type
                for (int i = 2; i < args.Length; i++)
                {
                    convert(args[i], Path.ChangeExtension(args[i], "C.xml"), conversionType);
                }
            }
            else
            {
                Console.WriteLine("ERROR: Incorrect Arguments!");
            }
            Console.Write("Press Any Key to Exit...");
            Console.ReadKey(true);
        }

        private static void convert(string path, string path2, int conversionType = 0)
        {
            // Get ConvertType
            XMLType convertType = XMLType.Text;
            if (convertType > 0 && !IntToXmlType(conversionType, out convertType))
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
                do
                {
                    Console.WriteLine("WARNING: Invalid conversion type entered because the file is already in this format!");
                    Console.WriteLine("Please enter the conversion type (excluding {0}), or type 'Exit' to quit: ", (int)file.type);
                    string cType = Console.ReadLine();
                    if (cType == "Exit")
                    {
                        return;
                    }
                    if (Int32.TryParse(cType, out conversionType) && !IntToXmlType(conversionType, out convertType, (int)file.type))
                    {
                        // If changing the Convert Type failed, reset it to its default value because IntToXmlType function 
                        // changed it to XmlType.Text
                        convertType = file.type;
                        //Console.WriteLine("{0}", convertType);
                    }
                } while (file.type == convertType);
            }

            // Convert
            try
            {
                file.Write(File.Open(path2, FileMode.Create, FileAccess.Write, FileShare.Read), convertType);
                Console.WriteLine("Success!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        private static bool IntToXmlType(int conversionType, out XMLType xmlType, int exclude = -1)
        {
            xmlType = XMLType.Text;
            if (exclude == conversionType)
            {
                return false;
            }
            if (Enum.IsDefined(typeof(XMLType), conversionType))
            {
                // Even if the conversion type is not defined, Enum.ToObject does not throw an exception
                xmlType = (XMLType)Enum.ToObject(typeof(XMLType), conversionType);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}