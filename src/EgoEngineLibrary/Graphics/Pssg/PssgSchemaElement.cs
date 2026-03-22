using System.Diagnostics;

namespace EgoEngineLibrary.Graphics.Pssg;

[DebuggerDisplay("{Name}")]
public class PssgSchemaElement
{
    public string Name { get; }

    public PssgSchemaElement? BaseElement
    {
        get;
        internal set
        {
            // Get inheritance depth to validate deep nesting and protect against circular references
            _ = GetInheritanceDepth();
            field = value;
        }
    }

    public PssgElementType DataType
    {
        get;
        internal set;
    }
    public int ElementsPerRow
    {
        get;
        internal set;
    }
    public string LinkAttributeName
    {
        get;
        internal set;
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

    internal int GetInheritanceDepth()
    {
        int depth = 0;
        var baseElement = BaseElement;
        while (baseElement is not null)
        {
            ++depth;
            baseElement = baseElement.BaseElement;

            if (depth > 20)
            {
                throw new InvalidOperationException("Inheritance depth of schema elements is too large.");
            }
        }
            
        return depth;
    }
}