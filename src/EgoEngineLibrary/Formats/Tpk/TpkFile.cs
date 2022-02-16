using System;
using System.IO;
using System.Text;

namespace EgoEngineLibrary.Formats.Tpk;

public class TpkFile
{
    private const int NameLength = 32;

    public TpkImageFormat Format { get; set; }

    public uint Width { get; set; }

    public uint Height { get; set; }

    public uint Unk11 { get; set; }

    public string Name { get; set; }

    public float Unk12 { get; set; }

    public uint MipMapCount { get; set; }

    public byte[] Data { get; set; }

    public TpkFile()
    {
        Format = TpkImageFormat.Dxt1;
        Unk11 = 0;
        Name = string.Empty;
        Unk12 = 1.0f;
        MipMapCount = 0;
        Data = Array.Empty<byte>();
    }

    public void Read(Stream stream)
    {
        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            reader.ReadBytes(40);
            Format = (TpkImageFormat)reader.ReadInt32();
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Unk11 = reader.ReadUInt32();
            var length = reader.ReadInt32();

            Name = Encoding.UTF8.GetString(reader.ReadBytes(NameLength)).TrimEnd('\0');

            Unk12 = reader.ReadSingle();
            MipMapCount = reader.ReadUInt32();
            Data = reader.ReadBytes(length);
        }
    }

    public void Write(Stream stream)
    {
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
        {
            writer.Write(0x65);
            writer.Write(0x14);
            writer.Write(0x50);
            writer.Write(0x64);

            writer.Write(0x01);
            writer.Write(0x01);
            writer.Write(0x05);
            writer.Write(0x02);

            writer.Write(0x02);
            writer.Write(0x02);

            writer.Write((int)Format);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(Unk11);
            writer.Write(Data.Length);

            var stringBytes = Encoding.UTF8.GetBytes(Name);
            for (var i = 0; i < NameLength; ++i)
            {
                if (i < stringBytes.Length)
                    writer.Write(stringBytes[i]);
                else
                    writer.Write((byte)0);
            }

            writer.Write(Unk12);
            writer.Write(MipMapCount);
            writer.Write(Data);
        }
    }
}
