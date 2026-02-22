using System.Text;

using EgoEngineLibrary.Data.Pkg;

namespace EgoEngineLibrary.Tests.Data.Pkg;

public class PkgFileTests
{
    [Fact]
    public void WriteJsonV0()
    {
        var expected = File.ReadAllText(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.wep.v0.json"));

        using var fs = File.Open(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.v0.wep"), FileMode.Open,
            FileAccess.Read, FileShare.Read);
        var pkg = PkgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pkg.WriteJson(ms);

        var actual = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void WriteJsonV1()
    {
        var expected = File.ReadAllText(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.wep.v1.json"));

        using var fs = File.Open(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.v1.wep"), FileMode.Open,
            FileAccess.Read, FileShare.Read);
        var pkg = PkgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pkg.WriteJson(ms);

        var actual = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ReadJsonV0()
    {
        var expected = File.ReadAllBytes(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.v0.wep"));

        using var fs = File.Open(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.wep.v0.json"), FileMode.Open,
            FileAccess.Read, FileShare.Read);
        var pkg = PkgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pkg.WritePkg(ms);

        var actual = ms.GetBuffer().AsSpan()[..(int)ms.Length];
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ReadJsonV0Legacy()
    {
        var expected = File.ReadAllBytes(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.v0.wep"));

        using var fs = File.Open(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.wep.v0legacy.json"), FileMode.Open,
            FileAccess.Read, FileShare.Read);
        var pkg = PkgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pkg.WritePkg(ms);

        var actual = ms.GetBuffer().AsSpan()[..(int)ms.Length];
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ReadJsonV1()
    {
        var expected = File.ReadAllBytes(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.v1.wep"));

        using var fs = File.Open(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.wep.v1.json"), FileMode.Open,
            FileAccess.Read, FileShare.Read);
        var pkg = PkgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pkg.WritePkg(ms);

        var actual = ms.GetBuffer().AsSpan()[..(int)ms.Length];
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ReadJsonV1Legacy()
    {
        var expected = File.ReadAllBytes(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.v1.wep"));

        using var fs = File.Open(Path.Combine(TestDataHelper.PkgPath, "ferrari!!!temp000.wep.v1legacy.json"), FileMode.Open,
            FileAccess.Read, FileShare.Read);
        var pkg = PkgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pkg.WritePkg(ms);

        var actual = ms.GetBuffer().AsSpan()[..(int)ms.Length];
        Assert.Equal(expected, actual);
    }
}