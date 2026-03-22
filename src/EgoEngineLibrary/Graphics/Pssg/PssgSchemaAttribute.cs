using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaAttribute
{
    public string Name { get; }
    public PssgAttributeType DataType
    {
        get;
        internal set;
    }

    public PssgSchemaAttribute(string name, PssgAttributeType dataType = PssgAttributeType.Unknown)
    {
        this.Name = name;
        this.DataType = dataType;
    }
}