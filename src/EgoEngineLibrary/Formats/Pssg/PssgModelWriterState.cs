using System;

namespace EgoEngineLibrary.Formats.Pssg
{
    public abstract class PssgModelWriterState
    {
        public uint DataBlockCount { get; set; }

        public uint RenderStreamCount { get; set; }
    }
}
