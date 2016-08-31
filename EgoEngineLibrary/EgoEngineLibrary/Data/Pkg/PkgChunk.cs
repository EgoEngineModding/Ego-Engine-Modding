using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Data.Pkg
{
    public abstract class PkgChunk
    {
        readonly PkgFile parentFile;

        public PkgFile ParentFile
        {
            get
            {
                return parentFile;
            }
        }

        protected abstract string ChunkType { get; }

        public PkgChunk(PkgFile parentFile)
        {
            this.parentFile = parentFile;
        }

        public abstract void Read(PkgBinaryReader reader);
        public abstract void Write(PkgBinaryWriter writer);
        internal abstract void UpdateOffsets();

        public abstract void FromJson(JsonTextReader reader);
        public abstract void ToJson(JsonTextWriter writer);
    }
}
