namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgNode
{
    internal static PssgSchemaElement Schema { get; } = new("NODE")
    {
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("stopTraversal", typeof(int)),
            new PssgSchemaAttribute("nickname", typeof(string)),
        }
    };
}