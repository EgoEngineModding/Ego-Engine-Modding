using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EgoEngineLibrary.Data.Pkg
{
    public abstract class PkgComplexValue : PkgChunk
    {
        public PkgComplexValue(PkgFile parentFile)
            : base(parentFile)
        {
        }
    }
}
