using EgoEngineLibrary.Formats.Tpk;

namespace Sandbox;

public class TpkSandbox
{
    public static void Run()
    {
        //foreach (var f in Directory.GetFiles(@"C:\Games\Steam\steamapps\common\Dirt 2\frontend\ts", "*.tpk", SearchOption.TopDirectoryOnly))
        //foreach (var f in Directory.GetFiles(@"C:\Games\Steam\steamapps\common\DiRT 3 Complete Edition\frontend\ts", "*.tpk", SearchOption.TopDirectoryOnly))
        //foreach (var f in Directory.GetFiles(@"C:\Games\Steam\steamapps\common\F1 2012\frontend\ts", "*.tpk", SearchOption.TopDirectoryOnly))
        foreach (var f in Directory.GetFiles(@"C:\Games\Steam\steamapps\common\F1 2014\frontend\ts", "*.tpk", SearchOption.TopDirectoryOnly))
        {
            var fName = Path.GetFileName(f);
            using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
            var tpk = new TpkFile();
            tpk.Read(fsi);

            if (!Enum.IsDefined(tpk.Format))
            {
                Console.WriteLine($"{fName}\t frmt {tpk.Format}");
            }

            if (tpk.Name != Path.GetFileNameWithoutExtension(f))
            {
                Console.WriteLine($"{fName}\t name {tpk.Name}");
            }

            if (tpk.Unk11 != 0)
            {
                Console.WriteLine($"{fName}\t u11 {tpk.Unk11}");
            }

            if (tpk.Unk12 != 1.0)
            {
                Console.WriteLine($"{fName}\t u12 {tpk.Unk12}");
            }
        }
    }
}
