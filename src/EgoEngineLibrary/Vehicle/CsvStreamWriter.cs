namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class CsvStreamWriter : System.IO.StreamWriter
    {
        public CsvStreamWriter(System.IO.Stream stream)
            : base(stream, new System.Text.UTF8Encoding(false))
        {
        }

        public void WriteEntryData(string type, object data)
        {
            switch (type)
            {
                case "int":
                    Write(string.Format(CultureInfo.InvariantCulture, "{0:d}", data));
                    break;
                case "float":
                case "double":
                    Write(string.Format(CultureInfo.InvariantCulture, "{0:f6}", data));
                    break;
                case "string":
                    Write(string.Format(CultureInfo.InvariantCulture, "{0}", data));
                    break;
                default:
                    throw new Exception("An entry in the ctfSchema file has an incorrect type!");
            }
            Write(',');
        }
    }
}
