using EgoEngineLibrary.Graphics.Pssg;

namespace EgoEngineLibrary.Tests.Graphics.Pssg;

public class PssgFileTests
{
    [Fact]
    public void ReadWriteBinary()
    {
        string filePath = Path.Combine(TestDataHelper.PssgPath, "ground_cover.pssg");
        var expected = File.ReadAllBytes(filePath);

        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var pssg = PssgFile.Open(fs);
        
        using var ms = new MemoryStream();
        pssg.WritePssg(ms);
        
        Assert.Equal(expected, ms.ToArray());
    }
}