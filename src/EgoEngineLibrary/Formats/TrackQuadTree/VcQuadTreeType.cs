namespace EgoEngineLibrary.Formats.TrackQuadTree;

public enum VcQuadTreeType
{
    /// <summary>
    /// Race Driver: Grid.
    /// Only difference with Dirt 2 is that the jpk container does not have qt.info so each vcqtc uses the entire track
    /// as its bounds.
    /// </summary>
    RaceDriverGrid,
    /// <summary>
    /// Dirt 2 and F1 2014.
    /// </summary>
    Dirt2,
    Dirt3,
    DirtShowdown
}
