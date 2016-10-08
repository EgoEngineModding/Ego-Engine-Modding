using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public class ErpGfxSurfaceRes1 : ErpFragmentData
    {
        public byte[] Data { get; set; }

        public override void FromFragment(ErpFragment fragment)
        {
            Data = fragment.GetDataArray(true);
        }

        public override void ToFragment(ErpFragment fragment)
        {
            fragment.SetData(Data);
        }
    }
}
