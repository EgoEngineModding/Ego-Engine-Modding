namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public class PssgRootNode
{
    internal static PssgSchemaElement Schema { get; } = new("ROOTNODE")
    {
        BaseElement = PssgNode.Schema
    };
}