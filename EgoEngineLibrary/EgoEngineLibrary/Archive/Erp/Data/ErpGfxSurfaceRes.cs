using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public class ErpGfxSurfaceRes : ErpResourceData
    {
        public ErpGfxSurfaceRes0 Fragment0 { get; set; }
        public ErpGfxSurfaceRes1 Fragment1 { get; set; }
        public ErpGfxSurfaceRes2 Frag2 { get; set; }

        public bool HasMips
        {
            get
            {
                return Frag2.Mips.Count > 0;
            }
        }

        public ErpGfxSurfaceRes()
        {
            Fragment0 = new ErpGfxSurfaceRes0();
            Fragment1 = new ErpGfxSurfaceRes1();
            Frag2 = new ErpGfxSurfaceRes2();
        }

        public override void FromResource(ErpResource resource)
        {
            Fragment0.FromFragment(resource.GetFragment("temp", 0));
            Fragment1.FromFragment(resource.GetFragment("temp", 1));

            ErpFragment mipsFragment = resource.TryGetFragment("mips", 0);
            if (mipsFragment != null)
            {
                Frag2.FromFragment(mipsFragment);
            }
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
            Fragment1.ToFragment(resource.GetFragment("temp", 1));

            ErpFragment mipsFragment = resource.TryGetFragment("mips", 0);
            if (mipsFragment != null)
            {
                Frag2.ToFragment(mipsFragment);
            }
        }
    }
}
