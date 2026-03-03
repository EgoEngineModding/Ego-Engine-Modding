using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaAttribute
{
    public string Name { get; }
    public Type DataType
    {
        get;
        set;
    }

    public PssgSchemaAttribute(string name, Type? dataType = null)
    {
        this.Name = name;
        this.DataType = dataType ?? typeof(Exception);
    }
}