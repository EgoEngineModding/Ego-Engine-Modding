using System.Numerics;

namespace EgoEngineLibrary.Track;

public class TrackTreesInstance
{
    public int ReferenceId { get; set; }
    public int InstanceId { get; set; }
    public Matrix4x4 Transform { get; set; }
    public Vector4 Color { get; set; }
    public float ShadowFactor { get; set; }
    public int Dynamic { get; set; }
    public int Landmark { get; set; }
    public int InstanceTag { get; set; }
    public string TodSpecific { get; set; } = string.Empty;
}