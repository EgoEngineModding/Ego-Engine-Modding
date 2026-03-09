namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgUserData : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("USERDATA")
    {
        CreateElement = (s, f, p) => new PssgUserData(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("object", PssgAttributeType.String),
        }
    };

    public PssgUserData(PssgFile file, PssgObject? parent)
        : this(Schema, file, parent)
    {
    }

    internal PssgUserData(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}