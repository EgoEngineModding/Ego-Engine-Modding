using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaElement
{
    public string Name { get; }
    public PssgSchemaElement? BaseElement { get; init; }
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
    }

    public PssgSchemaElement(string name, Type? dataType = null)
    {
        this.Name = name;
        this.DataType = dataType ?? typeof(Exception);
        this.ElementsPerRow = 32;
        this.LinkAttributeName = string.Empty;
        this.Attributes = new List<PssgSchemaAttribute>();
    }
}