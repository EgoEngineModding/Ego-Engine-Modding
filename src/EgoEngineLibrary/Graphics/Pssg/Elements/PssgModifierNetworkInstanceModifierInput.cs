namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgModifierNetworkInstanceModifierInput : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("MODIFIERNETWORKINSTANCEMODIFIERINPUT", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgModifierNetworkInstanceModifierInput(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("source", PssgAttributeType.Int),
            new PssgSchemaAttribute("stream", PssgAttributeType.Int),
        }
    };

    public byte Source
    {
        get => GetAttributeValue<byte>(Schema.Attributes[0].Name);
        set => AddAttribute(Schema.Attributes[0].Name, value);
    }

    public byte Stream
    {
        get => GetAttributeValue<byte>(Schema.Attributes[1].Name);
        set => AddAttribute(Schema.Attributes[1].Name, value);
    }

    public PssgModifierNetworkInstanceModifierInput(PssgFile file, PssgModifierNetworkInstance parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgModifierNetworkInstanceModifierInput(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}