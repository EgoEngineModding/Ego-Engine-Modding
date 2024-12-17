using EgoEngineLibrary.Data.Pkg;
using EgoEngineLibrary.Formats.Tpk;
using EgoEngineLibrary.Graphics.Dds;
using EgoEngineLibrary.Language;
using EgoEngineLibrary.Xml;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Xml;

using ConsoleAppFramework;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ZLogger;

namespace EgoFileConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(x =>
            {
                x.ClearProviders();
                x.SetMinimumLevel(LogLevel.Trace);
                x.AddZLoggerConsole();
            });
            services.AddSingleton<TrackQuadTreeApp>();

            using var serviceProvider = services.BuildServiceProvider();
            ConsoleApp.ServiceProvider = serviceProvider;
            
            var topLogger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
            ConsoleApp.Log = x => topLogger.LogInformation(x);
            ConsoleApp.LogError = x => topLogger.LogError(x);

            topLogger.LogInformation("--- {AppTitle} ---", Properties.Resources.AppTitleLong);
            topLogger.LogInformation("Drag and drop one or more files on the EXE to convert.");
            topLogger.LogInformation("Use --help to display a help message for any command");
            Console.WriteLine();

            var app = ConsoleApp.Create();
            app.UseFilter<UserPauseFilter>();
            app.Add("", Root);
            app.Add<TrackQuadTreeApp>("track-qt");
            app.Run(args);
        }

        /// <summary>
        /// Converts each input file by automatically determining the file type.
        /// </summary>
        /// <param name="filePaths">The space-separated input file paths to convert.</param>
        static void Root([Argument]params string[] filePaths)
        {
            if (filePaths.Length == 0)
            {
                Console.WriteLine("No input arguments were found!");
            }

            foreach (string f in filePaths)
            {
                try
                {
                    Console.WriteLine("Processing " + Path.GetFileName(f) + "...");

                    Convert(f);
                }
                catch (Exception ex) when (!System.Diagnostics.Debugger.IsAttached)
                {
                    Console.WriteLine("Failed to convert the file!");
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        private static void Convert(string f)
        {
            var fileName = Path.GetFileName(f);
            var ext = Path.GetExtension(f);
            string magic;
            string xmlMagic;
            using (var fs = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PkgBinaryReader reader = new PkgBinaryReader(fs);
                magic = reader.ReadString(4);

                // Skip first byte since BXMLBig starts with \0 causing empty string
                reader.Seek(1, SeekOrigin.Begin);
                xmlMagic = reader.ReadString(3);
            }
            
            if (xmlMagic == "\"Rr" || xmlMagic == "BXM")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                XmlFile file = new XmlFile(fsi);
                using var fso = File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read);
                file.Write(fso, XmlType.Text);
                Console.WriteLine("Success! XML converted.");
            }
            else if (magic == "LNGT")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                LngFile file = new LngFile(fsi);
                using var fso = File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read);
                file.WriteXml(fso);
                Console.WriteLine("Success! Lng converted.");
            }
            else if (magic == "!pkg")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                PkgFile file = PkgFile.ReadPkg(fsi);
                using var fso = File.Open(f + ".json", FileMode.Create, FileAccess.Write, FileShare.Read);
                file.WriteJson(fso);
                Console.WriteLine("Success! Pkg converted.");
            }
            else if (ext == ".tpk")
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var tpk = new TpkFile();
                tpk.Read(fsi);
                Console.WriteLine($"Tpk name '{tpk.Name}', image format '{tpk.Format}'.");
                var dds = tpk.ToDds();
                using var fso = File.Open(f + ".dds", FileMode.Create, FileAccess.Write, FileShare.Read);
                dds.Write(fso, -1);
                Console.WriteLine("Success! Tpk converted.");
            }
            else if (fileName.EndsWith(".tpk.dds"))
            {
                using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                var dds = new DdsFile(fsi);
                var tpk = new TpkFile()
                {
                    Name = fileName.Remove(fileName.IndexOf('.'))
                };
                tpk.FromDds(dds);
                using var fso = File.Open(f + ".tpk", FileMode.Create, FileAccess.Write, FileShare.Read);
                tpk.Write(fso);
                Console.WriteLine("Success! DDS converted.");
            }
            else if (fileName.EndsWith("track.jpk", StringComparison.InvariantCultureIgnoreCase))
            {
                var qtApp = ConsoleApp.ServiceProvider!.GetRequiredService<TrackQuadTreeApp>();
                qtApp.VcToGltf(f);
            }
            else
            {
                bool isJSON = false;
                JsonException jsonEx = null;
                try
                {
                    using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                    PkgFile pkgFile = PkgFile.ReadJson(fsi);
                    using var fso = File.Open(f + ".pkg", FileMode.Create, FileAccess.Write, FileShare.Read);
                    pkgFile.WritePkg(fso);
                    Console.WriteLine("Success! JSON converted.");
                    isJSON = true;
                }
                catch (JsonException e)
                {
                    jsonEx = e;
                }

                if (!isJSON)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    try
                    {
                        xmlDoc.Load(f);
                    }
                    catch (XmlException e)
                    {
                        throw new AggregateException("Could not determine the file type! Showing json, and xml errors: ", jsonEx, e);
                    }

                    if (xmlDoc.DocumentElement.Name == "language")
                    {
                        using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                        DataSet dataSet = new DataSet("language");
                        dataSet.ReadXml(fsi, XmlReadMode.ReadSchema);
                        LngFile file = new LngFile(dataSet);
                        using var fso = File.Open(f + ".lng", FileMode.Create, FileAccess.Write, FileShare.Read);
                        file.Write(fso);
                        Console.WriteLine("Success! XML converted.");
                    }
                    else
                    {
                        using var fsi = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                        XmlFile file = new XmlFile(fsi);
                        using var fso = File.Open(f + ".xml", FileMode.Create, FileAccess.Write, FileShare.Read);
                        file.Write(fso);
                        Console.WriteLine("Success! XML converted.");
                    }
                }
            }
        }

        private static void TpkTest()
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
}
