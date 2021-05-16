using System;
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
            return Create(parentFile, reader.ReadString(4));
        }
        public static PkgData Create(PkgFile parentFile, string type)
        {
            return type switch
            {
                "stri" => new PkgStringData(parentFile),
                "woid" => new PkgWoidData(parentFile),
                "mat4" => new PkgMat4Data(parentFile),
                "fp32" => new PkgFp32Data(parentFile),
                "bool" => new PkgBoolData(parentFile),
                "rgba" => new PkgRgbaData(parentFile),
                "shnm" => new PkgShnmData(parentFile),
                "vec3" => new PkgVec3Data(parentFile),
                "ui32" => new PkgUi32Data(parentFile),
                "si32" => new PkgSi32Data(parentFile),
                "ui64" => new PkgUi64Data(parentFile),
                "si64" => new PkgSi64Data(parentFile),
                "vec4" => new PkgVec4Data(parentFile),
                "quat" => new PkgQuatData(parentFile),
                "bbox" => new PkgBboxData(parentFile),
                "ui16" => new PkgUi16Data(parentFile),
                _ => throw new Exception("Data type not supported! " + type),
            };
        }

        public abstract string GetData(int index);
        public abstract int SetData(string data);

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
