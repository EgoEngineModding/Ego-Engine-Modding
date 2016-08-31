using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Numerics;
using System.IO;

namespace EgoEngineLibrary.Data.Pkg
{
    public class PkgByteData : PkgData
    {
        string type;
        readonly List<string> woidData;
        readonly List<Matrix4x4> mat4Data;
        readonly List<float> fp32Data;
        readonly List<bool> boolData;
        readonly List<UInt32> rgbaData;
        readonly List<string> shnmData;
        readonly List<Vector3> vec3Data;
        readonly List<UInt32> ui32Data;
        readonly List<Int32> si32Data;
        readonly List<Vector4> vec4Data;

        protected override string ChunkType
        {
            get
            {
                return "!vbi";
            }
        }

        public override string Type
        {
            get { return type; }
        }

        internal PkgByteData(PkgFile parentFile)
            : base(parentFile)
        {
            woidData = new List<string>();
            mat4Data = new List<Matrix4x4>();
            fp32Data = new List<float>();
            boolData = new List<bool>();
            rgbaData = new List<uint>();
            shnmData = new List<string>();
            vec3Data = new List<Vector3>();
            ui32Data = new List<uint>();
            si32Data = new List<int>();
            vec4Data = new List<Vector4>();
        }
        public PkgByteData(PkgFile parentFile, string type)
           : this(parentFile)
        {
            this.type = type;
        }

        public override void Read(PkgBinaryReader reader)
        {
            type = reader.ReadString(4);
            UInt32 numData = reader.ReadUInt32();
            UInt32 bytesPerData = reader.ReadUInt32();

            switch (type)
            {
                case "woid":
                    for (int i = 0; i < numData; ++i)
                    {
                        woidData.Add(Convert.ToBase64String(reader.ReadBytes(16), Base64FormattingOptions.None));
                    }
                    break;
                case "mat4":
                    for (int i = 0; i < numData; ++i)
                    {
                        Matrix4x4 mat4 = new Matrix4x4();
                        mat4.M11 = reader.ReadSingle();
                        mat4.M12 = reader.ReadSingle();
                        mat4.M13 = reader.ReadSingle();
                        mat4.M14 = reader.ReadSingle();
                        mat4.M21 = reader.ReadSingle();
                        mat4.M22 = reader.ReadSingle();
                        mat4.M23 = reader.ReadSingle();
                        mat4.M24 = reader.ReadSingle();
                        mat4.M31 = reader.ReadSingle();
                        mat4.M32 = reader.ReadSingle();
                        mat4.M33 = reader.ReadSingle();
                        mat4.M34 = reader.ReadSingle();
                        mat4.M41 = reader.ReadSingle();
                        mat4.M42 = reader.ReadSingle();
                        mat4.M43 = reader.ReadSingle();
                        mat4.M44 = reader.ReadSingle();
                        mat4Data.Add(mat4);
                    }
                    break;
                case "fp32":
                    for (int i = 0; i < numData; ++i)
                    {
                        fp32Data.Add(reader.ReadSingle());
                    }
                    break;
                case "bool":
                    for (int i = 0; i < numData; ++i)
                    {
                        boolData.Add(reader.ReadBoolean());
                    }
                    break;
                case "rgba":
                    for (int i = 0; i < numData; ++i)
                    {
                        rgbaData.Add(reader.ReadUInt32());
                    }
                    break;
                case "shnm":
                    for (int i = 0; i < numData; ++i)
                    {
                        shnmData.Add(reader.ReadString(16));
                    }
                    break;
                case "vec3":
                    for (int i = 0; i < numData; ++i)
                    {
                        Vector3 vec3 = new Vector3();
                        vec3.X = reader.ReadSingle();
                        vec3.Y = reader.ReadSingle();
                        vec3.Z = reader.ReadSingle();
                        reader.Seek(4, SeekOrigin.Current);
                        vec3Data.Add(vec3);
                    }
                    break;
                case "ui32":
                    for (int i = 0; i < numData; ++i)
                    {
                        ui32Data.Add(reader.ReadUInt32());
                    }
                    break;
                case "si32":
                    for (int i = 0; i < numData; ++i)
                    {
                        si32Data.Add(reader.ReadInt32());
                    }
                    break;
                case "vec4":
                    for (int i = 0; i < numData; ++i)
                    {
                        Vector4 vec4 = new Vector4();
                        vec4.X = reader.ReadSingle();
                        vec4.Y = reader.ReadSingle();
                        vec4.Z = reader.ReadSingle();
                        vec4.W = reader.ReadSingle();
                        vec4Data.Add(vec4);
                    }
                    break;
                default:
                    throw new Exception("Data type not supported! " + type);
            }
        }

        public override void Write(PkgBinaryWriter writer)
        {
            writer.Write(new byte[GetPaddingLength((Int32)writer.BaseStream.Position)]);
            writer.Write(ChunkType, 4);
            writer.Write(type, 4);

            switch (type)
            {
                case "woid":
                    writer.Write((UInt32)woidData.Count);
                    writer.Write((UInt32)16);
                    foreach (string woid in woidData)
                    {
                        writer.Write(Convert.FromBase64String(woid));
                    }
                    break;
                case "mat4":
                    writer.Write((UInt32)mat4Data.Count);
                    writer.Write((UInt32)64);
                    foreach (Matrix4x4 m4 in mat4Data)
                    {
                        writer.Write(m4.M11); writer.Write(m4.M12); writer.Write(m4.M13); writer.Write(m4.M14);
                        writer.Write(m4.M21); writer.Write(m4.M22); writer.Write(m4.M23); writer.Write(m4.M24);
                        writer.Write(m4.M31); writer.Write(m4.M32); writer.Write(m4.M33); writer.Write(m4.M34);
                        writer.Write(m4.M41); writer.Write(m4.M42); writer.Write(m4.M43); writer.Write(m4.M44);
                    }
                    break;
                case "fp32":
                    writer.Write((UInt32)fp32Data.Count);
                    writer.Write((UInt32)4);
                    foreach (float f in fp32Data)
                    {
                        writer.Write(f);
                    }
                    break;
                case "bool":
                    writer.Write((UInt32)boolData.Count);
                    writer.Write((UInt32)1);
                    foreach (bool b in boolData)
                    {
                        writer.Write(b);
                    }
                    break;
                case "rgba":
                    writer.Write((UInt32)rgbaData.Count);
                    writer.Write((UInt32)4);
                    foreach (UInt32 r in rgbaData)
                    {
                        writer.Write(r);
                    }
                    break;
                case "shnm":
                    writer.Write((UInt32)shnmData.Count);
                    writer.Write((UInt32)16);
                    foreach (string s in shnmData)
                    {
                        writer.Write(s, 16);
                    }
                    break;
                case "vec3":
                    writer.Write((UInt32)vec3Data.Count);
                    writer.Write((UInt32)16);
                    foreach (Vector3 v in vec3Data)
                    {
                        writer.Write(v.X);
                        writer.Write(v.Y);
                        writer.Write(v.Z);
                        writer.Write((UInt32)0);
                    }
                    break;
                case "ui32":
                    writer.Write((UInt32)ui32Data.Count);
                    writer.Write((UInt32)4);
                    foreach (UInt32 u in ui32Data)
                    {
                        writer.Write(u);
                    }
                    break;
                case "si32":
                    writer.Write((UInt32)si32Data.Count);
                    writer.Write((UInt32)4);
                    foreach (Int32 s in si32Data)
                    {
                        writer.Write(s);
                    }
                    break;
                case "vec4":
                    writer.Write((UInt32)vec4Data.Count);
                    writer.Write((UInt32)16);
                    foreach (Vector4 v in vec4Data)
                    {
                        writer.Write(v.X);
                        writer.Write(v.Y);
                        writer.Write(v.Z);
                        writer.Write(v.W);
                    }
                    break;
                default:
                    throw new Exception("Data type not supported! " + type);
            }
        }

        internal override void UpdateOffsets()
        {
            Int32 offsetAdjust = 16;

            switch (type)
            {
                case "woid":
                    offsetAdjust += (Int32)woidData.Count * 16;
                    break;
                case "mat4":
                    offsetAdjust += (Int32)mat4Data.Count * 64;
                    break;
                case "fp32":
                    offsetAdjust += (Int32)fp32Data.Count * 4;
                    break;
                case "bool":
                    offsetAdjust += (Int32)boolData.Count;
                    break;
                case "rgba":
                    offsetAdjust += (Int32)rgbaData.Count * 4;
                    break;
                case "shnm":
                    offsetAdjust += (Int32)shnmData.Count * 16;
                    break;
                case "vec3":
                    offsetAdjust += (Int32)vec3Data.Count * 16;
                    break;
                case "ui32":
                    offsetAdjust += (Int32)ui32Data.Count * 4;
                    break;
                case "si32":
                    offsetAdjust += (Int32)si32Data.Count * 4;
                    break;
                case "vec4":
                    offsetAdjust += (Int32)vec4Data.Count * 16;
                    break;
                default:
                    throw new Exception("Data type not supported! " + type);
            }

            PkgValue._offset += offsetAdjust;
        }

        public override string GetData(PkgOffsetType offsetType)
        {
            switch (type)
            {
                case "woid":
                    return type + " " + woidData[offsetType.Offset];
                case "mat4":
                    Matrix4x4 m = mat4Data[offsetType.Offset];
                    return type + " " + string.Format("{0},{1},{2},{3};{4},{5},{6},{7};{8},{9},{10},{11};{12},{13},{14},{15}",
                        m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
                case "fp32":
                    return type + " " + fp32Data[offsetType.Offset];
                case "bool":
                    return type + " " + boolData[offsetType.Offset];
                case "rgba":
                    return type + " " + rgbaData[offsetType.Offset].ToString("X");
                case "shnm":
                    return type + " " + shnmData[offsetType.Offset];
                case "vec3":
                    Vector3 vec3 = vec3Data[offsetType.Offset];
                    return type + " " + string.Format("{0},{1},{2}", vec3.X, vec3.Y, vec3.Z);
                case "ui32":
                    return type + " " + ui32Data[offsetType.Offset];
                case "si32":
                    return type + " " + si32Data[offsetType.Offset];
                case "vec4":
                    Vector4 vec4 = vec4Data[offsetType.Offset];
                    return type + " " + string.Format("{0},{1},{2},{3}", vec4.X, vec4.Y, vec4.Z, vec4.W);
                default:
                    throw new Exception("Data type not supported! " + type);
            }
        }
        public override void SetData(string data, PkgOffsetType offsetType)
        {
            string type = data.Remove(4);
            data = data.Substring(5);

            int index;
            switch (type)
            {
                case "woid":
                    index = woidData.IndexOf(data);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = woidData.Count;
                        woidData.Add(data);
                    }
                    break;
                case "mat4":
                    string[] s = data.Split(',', ';');
                    Matrix4x4 m = new Matrix4x4(
                        float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]),
                        float.Parse(s[4]), float.Parse(s[5]), float.Parse(s[6]), float.Parse(s[7]),
                        float.Parse(s[8]), float.Parse(s[9]), float.Parse(s[10]), float.Parse(s[11]),
                        float.Parse(s[12]), float.Parse(s[13]), float.Parse(s[14]), float.Parse(s[15]));
                    index = mat4Data.IndexOf(m);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = mat4Data.Count;
                        mat4Data.Add(m);
                    }
                    break;
                case "fp32":
                    float res = Single.Parse(data);
                    index = fp32Data.IndexOf(res);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = fp32Data.Count;
                        fp32Data.Add(res);
                    }
                    break;
                case "bool":
                    bool bl = bool.Parse(data);
                    index = boolData.IndexOf(bl);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = boolData.Count;
                        boolData.Add(bl);
                    }
                    break;
                case "rgba":
                    UInt32 rgba = UInt32.Parse(data);
                    index = rgbaData.IndexOf(rgba);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = rgbaData.Count;
                        rgbaData.Add(rgba);
                    }
                    break;
                case "shnm":
                    index = shnmData.IndexOf(data);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = shnmData.Count;
                        shnmData.Add(data);
                    }
                    break;
                case "vec3":
                    string[] vec3s = data.Split(',');
                    Vector3 vec3 = new Vector3(float.Parse(vec3s[0]), float.Parse(vec3s[1]), float.Parse(vec3s[2]));
                    index = vec3Data.IndexOf(vec3);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = vec3Data.Count;
                        vec3Data.Add(vec3);
                    }
                    break;
                case "ui32":
                    UInt32 ui32 = UInt32.Parse(data);
                    index = ui32Data.IndexOf(ui32);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = ui32Data.Count;
                        ui32Data.Add(ui32);
                    }
                    break;
                case "si32":
                    Int32 si32 = Int32.Parse(data);
                    index = si32Data.IndexOf(si32);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = si32Data.Count;
                        si32Data.Add(si32);
                    }
                    break;
                case "vec4":
                    string[] vec4s = data.Split(',');
                    Vector4 vec4 = new Vector4(float.Parse(vec4s[0]), float.Parse(vec4s[1]), float.Parse(vec4s[2]), float.Parse(vec4s[3]));
                    index = vec4Data.IndexOf(vec4);
                    if (index >= 0)
                    {
                        offsetType.Offset = index;
                    }
                    else
                    {
                        offsetType.Offset = vec4Data.Count;
                        vec4Data.Add(vec4);
                    }
                    break;
                default:
                    throw new Exception("Data type not supported! " + type);
            }
        }

        public override void FromJson(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }

        public override void ToJson(JsonTextWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
