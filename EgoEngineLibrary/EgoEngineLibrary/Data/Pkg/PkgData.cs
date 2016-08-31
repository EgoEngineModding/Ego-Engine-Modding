using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public abstract class PkgData : PkgComplexValue
    {
        public abstract string Type { get; }

        public PkgData(PkgFile parentFile) : base(parentFile)
        {
        }

        public abstract string GetData(PkgOffsetType offsetType);
        public abstract void SetData(string data, PkgOffsetType offsetType);

        public int GetPaddingLength(Int32 offset)
        {
            int padding;

            switch (Type)
            {
                case "stri":
                case "fp32":
                case "bool":
                case "rgba":
                case "ui32":
                case "si32":
                    padding = (-offset) & 3;
                    break;
                case "woid":
                case "shnm":
                case "vec3":
                case "vec4":
                    padding = (-offset) & 15;
                    break;
                case "mat4":
                    padding = (-offset) & 63;
                    break;
                default:
                    throw new Exception("Data type not supported! " + Type);
            }

            return padding;
        }
    }
}
