using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg.Data
{
    public abstract class PkgData : PkgComplexValue
    {
        public abstract string Type { get; }

        protected override string ChunkType
        {
            get
            {
                return "!vbi";
            }
        }

        public abstract int Align { get; }

        public PkgData(PkgFile parentFile)
            : base(parentFile)
        {
        }

        public static PkgData Create(PkgBinaryReader reader, PkgFile parentFile)
        {
            return PkgData.Create(parentFile, reader.ReadString(4));
        }
        public static PkgData Create(PkgFile parentFile, string type)
        {
            switch (type)
            {
                case "stri":
                    return new PkgStringData(parentFile);
                case "woid":
                    return new PkgWoidData(parentFile);
                case "mat4":
                    return new PkgMat4Data(parentFile);
                case "fp32":
                    return new PkgFp32Data(parentFile);
                case "bool":
                    return new PkgBoolData(parentFile);
                case "rgba":
                    return new PkgRgbaData(parentFile);
                case "shnm":
                    return new PkgShnmData(parentFile);
                case "vec3":
                    return new PkgVec3Data(parentFile);
                case "ui32":
                    return new PkgUi32Data(parentFile);
                case "si32":
                    return new PkgSi32Data(parentFile);
                case "ui64":
                    return new PkgUi64Data(parentFile);
                case "si64":
                    return new PkgSi64Data(parentFile);
                case "vec4":
                    return new PkgVec4Data(parentFile);
                case "quat":
                    return new PkgQuatData(parentFile);
                default:
                    throw new Exception("Data type not supported! " + type);
            }
        }

        public abstract string GetData(Int32 index);
        public abstract Int32 SetData(string data);

        public override void ToJson(JsonTextWriter writer)
        {
            throw new NotImplementedException();
        }
        public override void FromJson(JsonTextReader reader)
        {
            throw new NotImplementedException();
        }

        public int GetPaddingLength(int offset)
        {
            return (-offset) & (Align - 1);
        }
    }
}
