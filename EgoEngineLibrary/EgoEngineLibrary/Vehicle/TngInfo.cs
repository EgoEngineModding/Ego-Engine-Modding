namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class TngInfo
    {
        public int Id;
        public string Name;
        public bool IsParent;

        public TngInfo()
        {
            Id = 0;
            Name = string.Empty;
            IsParent = false;
        }
        public TngInfo(TngBinaryReader reader, TngFile file)
        {
            Id = (int)reader.BaseStream.Position - 12;
            Name = reader.ReadTerminatedString(0x00);
            IsParent = Name.Contains('/');

            file.TngInfo.Add(Id, this);
        }
    }
}
