namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLodRenderInstanceList : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("LODRENDERINSTANCELIST", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLodRenderInstanceList(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("lod", PssgAttributeType.Float),
        }
    };

    public float Lod
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }
    
    public IEnumerable<PssgRenderInstance> RenderInstances => ChildElements.OfType<PssgRenderInstance>();

    public PssgLodRenderInstanceList(PssgFile file, PssgLodRenderInstances parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLodRenderInstanceList(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}