namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgModifierNetworkInstanceUniqueModifierInput : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("MODIFIERNETWORKINSTANCEUNIQUEMODIFIERINPUT", PssgElementType.UInt)
    {
        CreateElement = (s, f, p) => new PssgModifierNetworkInstanceUniqueModifierInput(s, f, p),
    };

    public PssgModifierNetworkInstanceUniqueModifierInput(PssgFile file, PssgModifierNetworkInstanceCompile parent)
        : base(Schema, file, parent)
    {
    }

    internal PssgModifierNetworkInstanceUniqueModifierInput(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}