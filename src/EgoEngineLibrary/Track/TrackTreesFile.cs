using System.Diagnostics;
using System.Numerics;
using EgoEngineLibrary.IO;
using EgoEngineLibrary.Text;

namespace EgoEngineLibrary.Track;

public class TrackTreesFile
{
    public TrackTreesFileType Type { get; set; } 
    public Vector3 BoundsMin { get; set; }
    public Vector3 BoundsMax { get; set; }

    public List<TrackTreesInstanceRef> InstanceRefs { get; set; } = [];

    public List<TrackTreesInstance> Instances { get; set; } = [];

    public static TrackTreesFile ReadBinary(EndianBinaryReader reader, TrackTreesFileType type)
    {
        var file = new TrackTreesFile { Type = type };
        if (type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            var version = reader.ReadInt32();
            var instanceListOffset = reader.ReadInt32();
            int numInstanceList = reader.ReadInt32();

            Debug.Assert(version == 0);
            Debug.Assert(instanceListOffset == 12);
            Debug.Assert(numInstanceList == 1);
            reader.BaseStream.Seek(instanceListOffset, SeekOrigin.Begin);
        }

        file.BoundsMin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        file.BoundsMax = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        if (type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            reader.ReadInt32();
        }

        reader.ReadInt32();

        if (type is TrackTreesFileType.Dirt2 or TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            // totalLandmarks
            reader.ReadInt32();
        }

        int instanceRefOffset = reader.ReadInt32();
        int numInstanceRef = reader.ReadInt32();
        int instanceOffset = 0;
        int numInstance = 0;
        if (type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            instanceOffset = reader.ReadInt32();
            numInstance = reader.ReadInt32();
        }

        reader.BaseStream.Seek(instanceRefOffset, SeekOrigin.Begin);
        for (var r = 0; r < numInstanceRef; ++r)
        {
            var filenameOffset = reader.ReadInt32();
            var filename = ReadStringAtOffset(filenameOffset);
            var instanceRef = new TrackTreesInstanceRef(filename);
            file.InstanceRefs.Add(instanceRef);

            if (type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                instanceRef.ReferenceId = reader.ReadInt32();
            }
            else
            {
                instanceRef.ReferenceId = r;
            }

            instanceRef.BoundsMin = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            instanceRef.BoundsMax = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            if (type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                instanceRef.Distant = reader.ReadInt32();
                instanceRef.MaxInstances = reader.ReadInt32();
            }
            else
            {
                instanceRef.MaxInstances = reader.ReadInt32();
                reader.ReadInt32();
                instanceOffset = reader.ReadInt32();
                numInstance = reader.ReadInt32();
                instanceRef.Distant = reader.ReadInt32();

                if (numInstance <= 0)
                {
                    continue;
                }

                var pos = reader.BaseStream.Position;
                reader.Seek(instanceOffset, SeekOrigin.Begin);
                for (var i = 0; i < numInstance; ++i)
                {
                    var instance = new TrackTreesInstance();
                    instance.ReferenceId = r;
                    instance.InstanceId = file.Instances.Count;
                    file.Instances.Add(instance);
                    instance.Transform = new Matrix4x4(
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                        reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1);
                    instance.Color = new Vector4(
                        reader.ReadByte() / (float)byte.MaxValue,
                        reader.ReadByte() / (float)byte.MaxValue,
                        reader.ReadByte() / (float)byte.MaxValue,
                        reader.ReadByte() / (float)byte.MaxValue);
                    if (type is TrackTreesFileType.Dirt2)
                    {
                        instance.ShadowFactor = reader.ReadSingle();
                        instance.Dynamic = reader.ReadInt32();
                        instance.Landmark = reader.ReadInt32();
                    }

                    instance.InstanceTag = instance.InstanceId;
                }

                reader.BaseStream.Position = pos;
            }
        }

        if (numInstance <= 0 ||
            type is not TrackTreesFileType.Dirt3 and not TrackTreesFileType.GridAutosport)
        {
            return file;
        }

        reader.Seek(instanceOffset, SeekOrigin.Begin);
        for (var i = 0; i < numInstance; ++i)
        {
            var instance = new TrackTreesInstance();
            file.Instances.Add(instance);
            instance.ReferenceId = reader.ReadInt32();
            instance.InstanceId = reader.ReadInt32();
            instance.Transform = new Matrix4x4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1);
            instance.Color = new Vector4(
                reader.ReadByte() / (float)byte.MaxValue,
                reader.ReadByte() / (float)byte.MaxValue,
                reader.ReadByte() / (float)byte.MaxValue,
                reader.ReadByte() / (float)byte.MaxValue);
            instance.ShadowFactor = reader.ReadSingle();
            instance.Dynamic = reader.ReadInt32();
            instance.Landmark = reader.ReadInt32();
            instance.InstanceTag = reader.ReadInt32();
            if (type is TrackTreesFileType.GridAutosport)
            {
                var todSpecificOffset = reader.ReadInt32();
                if (todSpecificOffset < 0)
                {
                    continue;
                }

                instance.TodSpecific = ReadStringAtOffset(todSpecificOffset);
            }
        }

        return file;

        string ReadStringAtOffset(int offset)
        {
            var pos = reader.BaseStream.Position;
            reader.Seek(offset, SeekOrigin.Begin);
            var str = reader.ReadNullTerminatedString();
            reader.BaseStream.Position = pos;
            return str;
        }
    }

    public void WriteBinary(EndianBinaryWriter writer)
    {
        int instanceRefOffset;
        int instanceOffset;
        int bytesPerInstance;
        switch (Type)
        {
            case TrackTreesFileType.RaceDriverGrid:
                instanceRefOffset = 36;
                instanceOffset = instanceRefOffset + 48 * InstanceRefs.Count;
                bytesPerInstance = 52;
                break;
            case TrackTreesFileType.Dirt2:
                instanceRefOffset = 40;
                instanceOffset = instanceRefOffset + 48 * InstanceRefs.Count;
                bytesPerInstance = 64;
                break;
            case TrackTreesFileType.Dirt3:
                instanceRefOffset = 64;
                instanceOffset = instanceRefOffset + 40 * InstanceRefs.Count;
                bytesPerInstance = 76;
                break;
            case TrackTreesFileType.GridAutosport:
                instanceRefOffset = 64;
                instanceOffset = instanceRefOffset + 40 * InstanceRefs.Count;
                bytesPerInstance = 80;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Type), Type, null);
        }

        var strings = InstanceRefs.Select(x => x.Filename)
            .Concat(Instances.Select(x => x.TodSpecific).Where(x => !string.IsNullOrEmpty(x)));
        var stringBuffer = writer.Encoding.BuildStringByteBuffer(strings);
        if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            writer.Write(0);
            writer.Write(12);
            writer.Write(1);
        }
        
        writer.Write(BoundsMin.X); writer.Write(BoundsMin.Y); writer.Write(BoundsMin.Z);
        writer.Write(BoundsMax.X); writer.Write(BoundsMax.Y); writer.Write(BoundsMax.Z);

        if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            writer.Write(InstanceRefs.Count);
        }
        
        writer.Write(Instances.Count);

        if (Type is TrackTreesFileType.Dirt2 or TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            // F1 2012 - 2014 set total landmarks to 0 no matter what. Ignoring that for now
            writer.Write(Instances.Count(i => i.Landmark != 0));
        }
        
        writer.Write(instanceRefOffset);
        writer.Write(InstanceRefs.Count);

        if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
        {
            writer.Write(instanceOffset);
            writer.Write(Instances.Count);
        }

        int instanceStartIndex = 0;
        int stringsOffset = instanceOffset + bytesPerInstance * Instances.Count;
        foreach (var instanceRef in InstanceRefs)
        {
            writer.Write(stringsOffset + stringBuffer.StringOffsetMap[instanceRef.Filename]);
            if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                writer.Write(instanceRef.ReferenceId);
            }
            
            writer.Write(instanceRef.BoundsMin.X); writer.Write(instanceRef.BoundsMin.Y);
            writer.Write(instanceRef.BoundsMin.Z);
            writer.Write(instanceRef.BoundsMax.X); writer.Write(instanceRef.BoundsMax.Y);
            writer.Write(instanceRef.BoundsMax.Z);

            if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                writer.Write(instanceRef.Distant);
                writer.Write(instanceRef.MaxInstances);
            }
            else
            {
                writer.Write(instanceRef.MaxInstances);
                writer.Write(instanceStartIndex);
                writer.Write(instanceOffset + instanceStartIndex * bytesPerInstance);

                var numInstances = Instances.Count(i => i.ReferenceId == instanceRef.ReferenceId);
                instanceStartIndex += numInstances;
                writer.Write(numInstances);
                writer.Write(instanceRef.Distant);
            }
        }

        foreach (var instance in Instances)
        {
            if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                writer.Write(instance.ReferenceId);
                writer.Write(instance.InstanceId);
            }

            for (var r = 0; r < 4; ++r)
            {
                for (var c = 0; c < 3; ++c)
                {
                    writer.Write(instance.Transform[r, c]);
                }
            }

            var color = instance.Color * byte.MaxValue + new Vector4(0.5f);
            writer.Write((byte)color.X);
            writer.Write((byte)color.Y);
            writer.Write((byte)color.Z);
            writer.Write((byte)color.W);

            if (Type is TrackTreesFileType.Dirt2 or TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                writer.Write(instance.ShadowFactor);
                writer.Write(instance.Dynamic);
                writer.Write(instance.Landmark);
            }

            if (Type is TrackTreesFileType.Dirt3 or TrackTreesFileType.GridAutosport)
            {
                writer.Write(instance.InstanceTag);
            }

            if (Type is TrackTreesFileType.GridAutosport)
            {
                if (stringBuffer.StringOffsetMap.TryGetValue(instance.TodSpecific, out var todSpecificOffset))
                {
                    writer.Write(stringsOffset + todSpecificOffset);
                }
                else
                {
                    writer.Write(-1);
                }
            }
        }

        writer.Write(stringBuffer.Buffer, 0, stringBuffer.BufferSize);
    }
}