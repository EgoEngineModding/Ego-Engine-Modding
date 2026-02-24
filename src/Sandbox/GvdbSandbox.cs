using System.Text;
using EgoEngineLibrary.IO.Hashing;

namespace Sandbox;

public static class GvdbSandbox
{
    private class GameValueTemplate(string folder, string templateName)
    {
        private readonly uint[] _instanceBitmask = new uint[8];
        
        public string Folder { get; } = folder;
        public string TemplateName { get; } = templateName;
        public int InstanceCount { get; private set; } = -1;

        public IEnumerable<string> GetAllPossibleKeys()
        {
            yield return $"{Folder}.{TemplateName}._metadata_";
            
            for (var i = 0; i < 8; ++i)
            {
                yield return $"{Folder}._{TemplateName}_instancebitmask{i}_";
            }
        }

        public GameValueTemplate[] GetAllPossibleInstances()
        {
            var templates = new GameValueTemplate[8 * 32];
            for (var i = 0; i < 8 * 32; ++i)
            {
                templates[i] = new GameValueTemplate(Folder, $"{TemplateName}_{i}");
            }
            
            return templates;
        }

        public string GetValueKey(string valuePath)
        {
            return $"{Folder}.{TemplateName}.{valuePath}";
        }

        public string GetName()
        {
            var key = $"{Folder}.{TemplateName}._metadata_";
            // TODO: get string value or return empty string
            return string.Empty;
        }

        public GameValueTemplate GetInstance(int index)
        {
            // TODO: get if it exists
            return new GameValueTemplate(Folder, $"{TemplateName}_{index}");
        }

        private void CacheBitmasks()
        {
            InstanceCount = 0;
            for (var i = 0; i < 8; ++i)
            {
                var key = $"{Folder}._{TemplateName}_instancebitmask{i}_";
                var bitmask = 0u; // TODO: get from gvdb
                for (var j = 0; j < 32; ++j)
                {
                    if ((bitmask >> j & 1) != 0)
                    {
                        InstanceCount++;
                    }
                }
                
                _instanceBitmask[i] = bitmask;
            }
        }
    }
    
    public static void Run(string[] args)
    {
        var driverCharacteristicsGvt =
            new GameValueTemplate("ai.driver_characteristics.2014", "driver_characteristic");
        var dcGvtKeys = driverCharacteristicsGvt.GetAllPossibleKeys().Concat(driverCharacteristicsGvt
            .GetAllPossibleInstances().SelectMany(x => x.GetAllPossibleKeys()
                .Append(x.GetValueKey("aggression"))
                .Append(x.GetValueKey("control"))
                .Append(x.GetValueKey("reaction_speed"))
                .Append(x.GetValueKey("speed_in"))
                .Append(x.GetValueKey("speed_out"))
                .Append(x.GetValueKey("mistakes"))
                .Append(x.GetValueKey("virtual_performance"))
            ));
        var gvt = new GameValueTemplate("ai.driver_characteristics.parameter_mappings", "driver_parameter_mapping");
        var dcPmKeys = gvt.GetAllPossibleKeys().Concat(gvt.GetAllPossibleInstances().SelectMany(x =>
            x.GetAllPossibleKeys()
                .Append(x.GetValueKey("mapped_characteristic"))
                .Append(x.GetValueKey("low_difficulty_low_characteristic_mapping"))
                .Append(x.GetValueKey("low_difficulty_high_characteristic_mapping"))
                .Append(x.GetValueKey("high_difficulty_low_characteristic_mapping"))
                .Append(x.GetValueKey("high_difficulty_high_characteristic_mapping"))));
        var stringsCsv = dcGvtKeys
            .Append("ai.driver_characteristics.2014.driver_characteristic_0._metadata_");
        var djb2StringMap = new Dictionary<uint, HashSet<string>>();
        foreach (var l in stringsCsv)
        {
            if (l.Length > 256)
            {
                continue;
            }
            
            var line = l.Trim(['"']);
            var hash = Djb2.HashToUInt32(Encoding.UTF8.GetBytes(line));
            if (djb2StringMap.TryGetValue(hash, out var hashSet))
            {
                hashSet.Add(line);
                continue;
            }
            
            djb2StringMap.Add(hash, [line]);
        }

        //Console.WriteLine(string.Join(", ", djb2StringMap.Values.SelectMany(x => x).Where(x => x.AsSpan().Count(".") > 1)));

        using var fs = File.Open(@"C:\Users\tasev\Downloads\gamevaluedata!!!temp000", FileMode.Open, FileAccess.Read,
            FileShare.Read);
        using var br = new BinaryReader(fs);
        br.ReadUInt32();
        var numProperties = br.ReadUInt32();
        var magicString = "PxA6v23AL0INY7sA1Ab701AA2A44c7"u8;
        var magicIndex = 0;
        for (var i = 0; i < numProperties; ++i)
        {
            var key = br.ReadUInt32();
            var type = br.ReadByte();
            var valLength = br.ReadUInt32();
            var valueBytes = br.ReadBytes(Convert.ToInt32(valLength));
            for (var j = 0; j < valLength; ++j)
            {
                valueBytes[j] -= magicString[magicIndex++ % 30];
            }
            
            var value = Encoding.ASCII.GetString(valueBytes);
            br.BaseStream.Seek(1, SeekOrigin.Current);
            
            

            if (djb2StringMap.TryGetValue(key, out var hashSet))
            {
                Console.WriteLine(string.Join(", ", hashSet.Append(value)));
                //Console.WriteLine(hashSet.First());
                if (hashSet.Count > 1)
                {
                    int a = 5;
                }
            }
            else
            {
                //Console.WriteLine(key);
            }
        }
    }
}