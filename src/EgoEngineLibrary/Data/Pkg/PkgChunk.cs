using System.Text.Json;

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

        public abstract void FromJson(ref Utf8JsonReader reader);
        public abstract void ToJson(Utf8JsonWriter writer);
    }
}
