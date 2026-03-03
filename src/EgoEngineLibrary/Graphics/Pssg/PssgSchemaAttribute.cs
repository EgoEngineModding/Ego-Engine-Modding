using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaAttribute
{
    public string Name
    {
        get;
        private set;
    }
    public Type DataType
    {
        get;
        set;
    }

    public PssgSchemaAttribute(string name)
    {
        this.Name = name;
        this.DataType = typeof(System.Exception);
    }
    public PssgSchemaAttribute(string name, Type dataType)
    {
        this.Name = name;
        this.DataType = dataType;
    }
}