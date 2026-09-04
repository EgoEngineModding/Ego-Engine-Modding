using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.IO;
using EgoEngineLibrary.Track;

namespace Sandbox;

public class TrackTreesSandbox
{
    public static void Run(string[] args)
    {
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Grid\tracks\", TrackTreesFileType.RaceDriverGrid);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\Dirt 2\tracks\", TrackTreesFileType.Dirt2);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2010\tracks\", TrackTreesFileType.Dirt2);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2011\tracks\", TrackTreesFileType.Dirt2);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2012\tracks\", TrackTreesFileType.Dirt2);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\f12013\tracks\", TrackTreesFileType.Dirt2);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\F1 2014\tracks\", TrackTreesFileType.Dirt2);
        var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT 3 Complete Edition\tracks\", TrackTreesFileType.Dirt3);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Showdown\tracks\", TrackTreesFileType.Dirt3);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\grid 2\tracks\", TrackTreesFileType.Dirt3);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\GRID Autosport\tracks\", TrackTreesFileType.GridAutosport);
        //var (folder, type) = (@"C:\Games\Steam\steamapps\common\DiRT Rally\tracks\", TrackTreesFileType.GridAutosport);
        var files = Utils.GetFiles("trees.bin", folder);
        foreach (var f in files)
        {
            try
            {
                using var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var ms = new MemoryStream();
                fs.CopyTo(ms);

                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new EndianBinaryReader(EndianBitConverter.Little, ms);
                var trees = TrackTreesFile.ReadBinary(reader, type);

                using var ms2 = new MemoryStream();
                using var writer = new EndianBinaryWriter(EndianBitConverter.Little, ms2);
                trees.WriteBinary(writer);

                if (!ms.ToArray().SequenceEqual(ms2.ToArray()))
                {
                    Console.WriteLine(f);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(f);
                Console.WriteLine(e);
            }
        }
    }

    private static void ConvertD2ToD3()
    {
        var f = @"C:\Games\Steam\steamapps\common\Dirt 2\tracks\croatia\croatia_rally\route_0\trees.bin";
        using var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new EndianBinaryReader(EndianBitConverter.Little, fs);
        var trees = TrackTreesFile.ReadBinary(reader, TrackTreesFileType.Dirt2);
            
        using var fso = File.Open(f + ".bin", FileMode.Create, FileAccess.Write, FileShare.Read);
        using var writer = new EndianBinaryWriter(EndianBitConverter.Little, fso);
        trees.Type = TrackTreesFileType.Dirt3;
        trees.WriteBinary(writer);
    }
}