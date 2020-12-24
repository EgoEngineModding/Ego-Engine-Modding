namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class CsvStreamReader : System.IO.StreamReader
    {
        public CsvStreamReader(System.IO.Stream stream)
            : base(stream, Encoding.UTF8)
        {
        }

        public object ReadEntryData(string type, string entry, string name)
        {
            switch (type)
            {
                case "int":
                    if (string.IsNullOrEmpty(entry))
                        return string.Empty;
                    return Convert.ToInt32(entry, CultureInfo.InvariantCulture);
                case "float":
                    if (string.IsNullOrEmpty(entry))
                        return string.Empty;
                    return Convert.ToSingle(entry, CultureInfo.InvariantCulture);
                case "double":
                    if (string.IsNullOrEmpty(entry))
                        return string.Empty;
                    return Convert.ToDouble(entry, CultureInfo.InvariantCulture);
                case "string":
                    return entry;
                default:
                    throw new Exception("An entry in the ctfSchema file has an incorrect type!");
            }
        }
    }
}
