using System.Numerics;

namespace EgoEngineLibrary.Track;

public class TrackTreesInstanceRef
{
    public string Filename { get; set; }
    public int ReferenceId { get; set; }
    
    public Vector3 BoundsMin { get; set; }
    public Vector3 BoundsMax { get; set; }
    
    public int Distant { get; set; }
    public int MaxInstances { get; set; }

    public TrackTreesInstanceRef(string filename)
    {
        Filename = filename;
    }
}