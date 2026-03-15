namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public abstract class PssgRenderInterfaceBound : PssgObject
{
    internal static new PssgSchemaElement Schema { get; } = new("RENDERINTERFACEBOUND", PssgElementType.None)
    {
        CreateElement = null,
        BaseElement = PssgObject.Schema,
        Attributes =
        {
            new PssgSchemaAttribute("localData", PssgAttributeType.Int),
            new PssgSchemaAttribute("isRenderTarget", PssgAttributeType.Int),
            new PssgSchemaAttribute("allocateSystem", PssgAttributeType.Int),
            new PssgSchemaAttribute("prioritizeRead", PssgAttributeType.Int),
            new PssgSchemaAttribute("automaticBind", PssgAttributeType.Int),
            new PssgSchemaAttribute("discardLocalAfterBind", PssgAttributeType.Int),
        }
    };

    internal PssgRenderInterfaceBound(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}