using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaElement
{
    public string Name { get; }
    public PssgSchemaElement? BaseElement { get; init; }
    public PssgElementType DataType
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

    internal Func<PssgSchemaElement, PssgFile, PssgElement?, PssgElement>? CreateElement
    {
        get;
        init;
    }

    public PssgSchemaElement(string name, PssgElementType dataType = PssgElementType.Unknown)
    {
        this.Name = name;
        this.DataType = dataType;
        this.ElementsPerRow = 32;
        this.LinkAttributeName = string.Empty;
        this.Attributes = new List<PssgSchemaAttribute>();
        this.CreateElement = (s, f, p) => new PssgElement(s, f, p);
    }

    internal PssgElement Create(PssgFile file, PssgElement? parent)
    {
        return CreateElement is null
            ? throw new InvalidOperationException($"Element '{Name}' cannot be created.")
            : CreateElement(this, file, parent);
    }
}