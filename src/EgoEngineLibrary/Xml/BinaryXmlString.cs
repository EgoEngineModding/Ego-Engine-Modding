using System;
using System.Collections.Generic;

namespace EgoEngineLibrary.Xml;

public class BinaryXmlString
{
    private string[] _values;

    public BinaryXmlString()
    {
        _values = Array.Empty<string>();
    }

    // Reads all the strings available in the binary xml file
    // First Part is string data
    // Second part is position index of string data, not used by program
    public void Read(XmlBinaryReader reader)
    {
        // Section 3
        reader.ReadInt32();
        var len = reader.ReadInt32();
        var endPos = len + reader.BaseStream.Position;
        var vals = new List<string>();
        while (reader.BaseStream.Position < endPos)
        {
            var str = reader.ReadTerminatedString();
            //if (!string.IsNullOrEmpty(str))
            //{
            vals.Add(str);
            //}
        }
        _values = vals.ToArray();
        //System.Windows.Forms.MessageBox.Show(values.Length.ToString());

        // Section 4
        reader.ReadInt32();
        var len2 = reader.ReadInt32();
        reader.Seek(len2, System.IO.SeekOrigin.Current);
    }

    public string this[int index] => _values[index];
}