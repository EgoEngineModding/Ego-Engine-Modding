using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public class ErpGfxSRVResource : ErpResourceData
    {
        public ErpGfxSRVResource0 Fragment0 { get; set; }

        public ErpGfxSurfaceRes SurfaceRes { get; set; }

        public ErpGfxSRVResource()
        {
            Fragment0 = new ErpGfxSRVResource0();
            SurfaceRes = new ErpGfxSurfaceRes();
        }

        public override void FromResource(ErpResource resource)
        {
            Fragment0.FromFragment(resource.GetFragment("temp", 0));
            SurfaceRes.FromResource(resource.ParentFile.FindResource(Fragment0.SurfaceResourceName));
        }

        public override void Read(ErpBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public override void Write(ErpBinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public override void ToResource(ErpResource resource)
        {
            Fragment0.ToFragment(resource.GetFragment("temp", 0));
            SurfaceRes.ToResource(resource.ParentFile.FindResource(Fragment0.SurfaceResourceName));
        }
    }
}
