using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaElement
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
    public int ElementsPerRow
    {
        get;
        set;
    }
    public string LinkAttributeName
    {
        get;
        set;
    }
    public List<PssgSchemaAttribute> Attributes
    {
        get;
        set;
    }

    public PssgSchemaElement(string name)
    {
        this.Name = name;
        this.DataType = typeof(System.Exception);
        this.ElementsPerRow = 32;
        this.LinkAttributeName = string.Empty;
        this.Attributes = new List<PssgSchemaAttribute>();
    }
    public PssgSchemaElement(string name, Type dataType)
    {
        this.Name = name;
        this.DataType = dataType;
        this.ElementsPerRow = 32;
        this.LinkAttributeName = string.Empty;
        this.Attributes = new List<PssgSchemaAttribute>();
    }
}