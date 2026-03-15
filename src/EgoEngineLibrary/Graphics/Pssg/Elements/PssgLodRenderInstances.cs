namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgLodRenderInstances : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("LODRENDERINSTANCES", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgLodRenderInstances(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("lodCount", PssgAttributeType.Int),
        }
    };

    public uint LodCount
    {
        get => GetAttributeValue<uint>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public IEnumerable<PssgLodRenderInstanceList> InstanceLists => ChildElements.OfType<PssgLodRenderInstanceList>();

    public PssgLodRenderInstances(PssgFile file, PssgLodVisibleRenderNode parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgLodRenderInstances(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}