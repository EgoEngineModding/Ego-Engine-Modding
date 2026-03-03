namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgObject
{
    internal static PssgSchemaElement Schema { get; } = new("XXX")
    {
        Attributes =
        {
            new PssgSchemaAttribute("id", typeof(string)),
        }
    };
}