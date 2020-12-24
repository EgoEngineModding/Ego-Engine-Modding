using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public abstract class ErpResourceData
    {
        public abstract void Read(ErpBinaryReader reader);
        public abstract void Write(ErpBinaryWriter writer);

        public abstract void FromResource(ErpResource resource);
        public abstract void ToResource(ErpResource resource);
    }
}
