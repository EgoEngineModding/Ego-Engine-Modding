using EgoEngineLibrary.Archive.Erp;
using EgoEngineLibrary.Formats.Erp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoErpArchiverConsole
{
    class Program
    {
        static StringBuilder _stringBuilder;

        static void Main(string[] args)
        {
            _stringBuilder = new StringBuilder();
            _stringBuilder.AppendLine("--- " + Properties.Resources.AppTitleLong + " ---");

            try
            {
                if (args.Length == 4)
                {
                    if (args[0] == "-e")
                    {
                        Export(args[1], args[2]);
                    }
                    else if (args[0] == "-i")
                    {
                        Import(args[1], args[2]);
                    }
                    
                    _stringBuilder.AppendLine();
                    _stringBuilder.AppendLine("Success!");
                }
                if (args.Length == 0)
                {
                    _stringBuilder.AppendLine("Incorrect input arguments!");
                    _stringBuilder.AppendLine("Export Example: -e file.erp .\\erpFolder EEAC_log.txt");
                    _stringBuilder.AppendLine("Import Example: -i file.erp .\\erpFolder EEAC_log.txt");
                }
            }
            catch (Exception ex)
            {
                _stringBuilder.AppendLine();
                _stringBuilder.AppendLine("Fail!");
                _stringBuilder.AppendLine(ex.ToString());
            }
            finally
            {
                try
                {
                    Console.Write(_stringBuilder.ToString());
                }
                catch { }
                try
                {
                    using (StreamWriter sw = new StreamWriter(File.Open(args[3], FileMode.Create, FileAccess.Write, FileShare.Read)))
                    {
                        sw.Write(_stringBuilder.ToString());
                    }
                }
                catch { }
            }
        }

        static void Export(string file, string folder)
        {
            ErpFile erp = new ErpFile();
            using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                erp.Read(fs);

            var statuProgress = new Progress<string>(status =>
            {
                _stringBuilder.Append(status);
            });

            var resourceExporter = new ErpResourceExporter();
            resourceExporter.Export(erp, folder, statuProgress, null);
        }

        static void Import(string file, string folder)
        {
            ErpFile erp = new ErpFile();
            using (var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                erp.Read(fs);

            var progressStatus = new Progress<string>(status =>
            {
                _stringBuilder.Append(status);
            });

            var resourceExporter = new ErpResourceExporter();
            resourceExporter.Import(erp, Directory.GetFiles(folder, "*", SearchOption.AllDirectories), progressStatus, null);

            using (var fs = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.Read))
                erp.Write(fs);
        }
    }
}
