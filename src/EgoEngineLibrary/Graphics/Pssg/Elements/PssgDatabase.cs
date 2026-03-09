namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgDatabase : PssgElement
{
    internal static PssgSchemaElement Schema { get; } = new("PSSGDATABASE", PssgElementType.None)
    {
        CreateElement = (s, f, p) => new PssgDatabase(s, f, p),
        Attributes =
        {
            new PssgSchemaAttribute("creator", PssgAttributeType.String),
            new PssgSchemaAttribute("creationMachine", PssgAttributeType.String),
            new PssgSchemaAttribute("creationDate", PssgAttributeType.String),
            new PssgSchemaAttribute("scale", PssgAttributeType.Float3),
            new PssgSchemaAttribute("up", PssgAttributeType.Float3),
        }
    };

    public PssgDatabase(PssgFile file)
        : this(Schema, file, null)
    {
    }

    internal PssgDatabase(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}