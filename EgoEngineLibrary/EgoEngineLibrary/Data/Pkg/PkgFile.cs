using MiscUtil.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgFile
    {
        PkgRootObject rootItem;

        public PkgRootObject RootItem
        {
            get
            {
                return rootItem;
            }
        }

        public PkgFile()
        {
            rootItem = new PkgRootObject(this);
        }

        public static PkgFile Open(Stream stream)
        {
            Byte[] header = new Byte[4];
            stream.Read(header, 0, 4);
            stream.Seek(0, SeekOrigin.Begin);
            string magic = Encoding.UTF8.GetString(header);
            
            if (magic == "!pkg")
            {
                return ReadPkg(stream);
            }
            else if (magic[0] == '{')
            {
                return ReadJson(stream);
            }
            else
            {
                throw new FileFormatException("This is not a package file!");
            }
        }
        public static PkgFile ReadPkg(Stream stream)
        {
            PkgFile file = new PkgFile();
            using (PkgBinaryReader reader = new PkgBinaryReader(stream))
            {
                file.RootItem.Read(reader);
            }
            return file;
        }
        public static PkgFile ReadJson(Stream stream)
        {
            PkgFile file = new PkgFile();
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(stream)))
            {
                file.RootItem.FromJson(reader);
            }
            return file;
        }

        public void Read(Stream stream)
        {
            rootItem = new PkgRootObject(this);
            using (PkgBinaryReader reader = new PkgBinaryReader(stream))
            {
                rootItem.Read(reader);
            }
            using (JsonTextWriter writer = new JsonTextWriter(new StreamWriter(File.Open(@"C:\Users\Petar\Desktop\f1111111111ttttttttt\f1_2016_vehicle_package\teams\ferrari\wep\temp.json", FileMode.Create))))
            {
                writer.Formatting = Formatting.Indented;
                rootItem.ToJson(writer);
            }
        }
        public void Write(Stream stream)
        {
            using (JsonTextReader reader = new JsonTextReader(new StreamReader(File.Open(@"C:\Users\Petar\Desktop\f1111111111ttttttttt\f1_2016_vehicle_package\teams\ferrari\wep\temp.json", FileMode.Open, FileAccess.Read, FileShare.Read))))
            {
                rootItem.FromJson(reader);
            }
            using (PkgBinaryWriter writer = new PkgBinaryWriter(File.Open(@"C:\Users\Petar\Desktop\f1111111111ttttttttt\f1_2016_vehicle_package\teams\ferrari\wep\temp.pkg", FileMode.Create)))
            {
                rootItem.Write(writer);
            }
            int temp = 5;
        }
    }
}
