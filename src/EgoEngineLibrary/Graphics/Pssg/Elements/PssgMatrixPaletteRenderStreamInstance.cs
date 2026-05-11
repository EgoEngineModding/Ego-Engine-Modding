namespace EgoEngineLibrary.Graphics.Pssg.Elements;

public abstract class PssgMatrixPaletteRenderStreamInstance : PssgRenderStreamInstance
{
    public abstract uint IndexOffset { get; set; }

    public abstract uint IndicesCountFromOffset { get; set; }

    internal PssgMatrixPaletteRenderStreamInstance(PssgSchemaElement schemaElement, PssgFile file, PssgElement? parent)
        : base(schemaElement, file, parent)
    {
    }
}