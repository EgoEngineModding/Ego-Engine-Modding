namespace EgoEngineLibrary.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class BinaryXmlString
    {
        private string[] values;

        // Reads all the strings available in the binary xml file
        // First Part is string data
        // Second part is position index of string data, not used by program
        public BinaryXmlString(XmlBinaryReader reader)
        {
            // Section 3
            reader.ReadInt32();
            int len = reader.ReadInt32();
            long endPos = len + reader.BaseStream.Position;
            List<string> vals = new List<string>();
            while (reader.BaseStream.Position < endPos)
            {
                string str = reader.ReadTerminatedString();
                //if (!string.IsNullOrEmpty(str))
                //{
                vals.Add(str);
                //}
            }
            values = vals.ToArray();
            //System.Windows.Forms.MessageBox.Show(values.Length.ToString());

            // Section 4
            reader.ReadInt32();
            int len2 = reader.ReadInt32();
            reader.Seek(len2, System.IO.SeekOrigin.Current);
        }

        public string this[int index]
        {
            get { return values[index]; }
        }
    }
}
