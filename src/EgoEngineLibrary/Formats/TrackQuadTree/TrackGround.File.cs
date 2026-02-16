using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using EgoEngineLibrary.Archive.Jpk;
using EgoEngineLibrary.Formats.TrackQuadTree.Static;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

public partial class TrackGround
{
    private const string InfoEntryName = "qt.info";

    public static VcQuadTreeTypeInfo Identify(JpkFile jpk)
    {
        var entry = jpk.Entries.FirstOrDefault(x => !x.Name.Equals(InfoEntryName));
        if (entry is null)
        {
            throw new InvalidOperationException("Cannot identify track ground type without a valid vcqtc file.");
        }

        var info = VcQuadTreeFile.Identify(entry.Data);
        if (info.Type is VcQuadTreeType.Dirt2 && TryGetQtInfo(jpk) is null)
        {
            return VcQuadTreeTypeInfo.Get(VcQuadTreeType.RaceDriverGrid);
        }

        return info;
    }

    public static TrackGround Load(JpkFile jpk, VcQuadTreeTypeInfo? typeInfo = null)
    {
        typeInfo ??= Identify(jpk);
        
        // Some files don't have qt.info (RD:G) so we'll compute bounds from entries
        var info = TryGetQtInfo(jpk);
        if (info is null && typeInfo.Type is not VcQuadTreeType.RaceDriverGrid)
        {
            throw new FileNotFoundException("qt.info not found in jpk.");
        }

        var minBounds = new Vector3(float.MaxValue);
        var maxBounds = new Vector3(float.MinValue);
        var maxSubSubDivisions = new int[TotalCells];
        foreach (var entry in jpk.Entries)
        {
            if (entry.Name.Equals(InfoEntryName))
            {
                continue;
            }

            var (x, z, level) = GetDataFromName(entry.Name);
            var topLevelCell = GetCellIndex(x, z);
            var subSubDivisions = level - GridSubdivisions;
            if (maxSubSubDivisions[topLevelCell] < subSubDivisions)
            {
                maxSubSubDivisions[topLevelCell] = subSubDivisions;
            }

            if (info is null)
            {
                minBounds = Vector3.Min(minBounds, GetMinBounds(entry.Data));
                maxBounds = Vector3.Max(maxBounds, GetMaxBounds(entry.Data));
            }
        }

        if (info is not null)
        {
            minBounds = GetMinBounds(info.Data);
            maxBounds = GetMaxBounds(info.Data);
        }

        var ground = new TrackGround(minBounds, maxBounds, maxSubSubDivisions);
        foreach (var entry in jpk.Entries)
        {
            if (entry.Name.Equals(InfoEntryName))
            {
                continue;
            }
            
            var (x, z, level) = GetDataFromName(entry.Name);
            var nodeWidth = GridWidth >> level;
            var qtc = new VcQuadTreeFile(entry.Data, typeInfo);
            ground.Set(qtc, x, z, x + nodeWidth - 1, z + nodeWidth - 1);
            Debug.Assert(qtc == ground.Get(x, z));
        }

        Debug.Assert(ground.TraverseGrid().Select(x => (x.X, x.Y, x.Level)).SequenceEqual(
            jpk.Entries.Where(x => !x.Name.Equals(InfoEntryName)).Select(x => GetDataFromName(x.Name))));

        return ground;

        Vector3 GetMinBounds(byte[] data)
        {
            return Unsafe.As<byte, Vector3>(ref data[0]);
        }
        Vector3 GetMaxBounds(byte[] data)
        {
            return Unsafe.As<byte, Vector3>(ref data[12]);
        }
    }

    public JpkFile Save()
    {
        VcQuadTreeType? type = null;
        var jpk = new JpkFile();
        foreach (var leaf in TraverseGrid())
        {
            var (qt, x, z, level) = leaf;
            type ??= qt.TypeInfo.Type;
            var name = GetNameFromData(x, z, level);

            var entry = new JpkEntry(jpk) { Name = name, Data = qt.Bytes };
            jpk.Entries.Add(entry);
            Debug.WriteLine($"{entry.Name} {entry.Data.Length} {x} {z} {level}");
        }

        if (type is not VcQuadTreeType.RaceDriverGrid)
        {
            var infoEntry = new JpkEntry(jpk) { Name = InfoEntryName, Data = new byte[24] };
            var bounds = MemoryMarshal.Cast<byte, Vector3>(infoEntry.Data.AsSpan(0, 24));
            bounds[0] = BoundsMin;
            bounds[1] = BoundsMax;
            jpk.Entries.Add(infoEntry);
        }

        return jpk;
    }

    private static JpkEntry? TryGetQtInfo(JpkFile jpk)
    {
        return jpk.Entries.FirstOrDefault(x => x.Name.Equals(InfoEntryName));
    }

    private static (int X, int Z, int Level) GetDataFromName(string name)
    {
        var xValue = GridWidth >> 1;
        var zValue = GridWidth >> 1;
        var x = 0;
        var z = 0;
        var level = 0;
        var nameBuf = name.AsSpan(2);
        while (nameBuf.Length >= 3 && nameBuf[0] == '_')
        {
            if (nameBuf[1] == '1')
            {
                x += xValue;
            }

            if (nameBuf[2] == '1')
            {
                z += zValue;
            }

            ++level;
            xValue >>= 1;
            zValue >>= 1;
            nameBuf = nameBuf[3..];
        }
        
        return (x, z, level);
    }

    private static string GetNameFromData(int x, int z, int level)
    {
        return string.Create(8 + level * 3, (x, z, level), static (chars, state) =>
        {
            var (x,z, level) = state;
            var mask = GridWidth >> 1;
            chars[0] = 'q';
            chars[1] = 't';
            for (var i = 2; i < level * 3 + 2; i += 3)
            {
                chars[i] = '_';
                chars[i + 1] = (x & mask) != 0 ? '1' : '0';
                chars[i + 2] = (z & mask) != 0 ? '1' : '0';
                mask >>= 1;
            }

            chars[^6] = '.';
            chars[^5] = 'v';
            chars[^4] = 'c';
            chars[^3] = 'q';
            chars[^2] = 't';
            chars[^1] = 'c';
        });
    }
}
