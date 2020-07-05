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

        public UInt32 Width => Fragment0.Width;
        public UInt32 Height => Fragment0.Height;
        public UInt32 LinearSize => GetLinearSize(Width, Height);

        public UInt32 MipWidth => Width * (UInt32)Math.Pow(2, Frag2.Mips.Count);
        public UInt32 MipHeight => Height * (UInt32)Math.Pow(2, Frag2.Mips.Count);
        public UInt64 MipLinearSize => HasMips ? Frag2.Mips[0].Size : 0;
        public bool HasMips => Frag2.Mips.Count > 0;
        public bool HasValidMips
        {
            get
            {
                UInt32 linSize = LinearSize;

                return linSize < MipLinearSize
                  && linSize * Math.Pow(2, Frag2.Mips.Count + Frag2.Mips.Count) == MipLinearSize;
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

            ErpFragment? mipsFragment = resource.TryGetFragment("mips", 0) ?? resource.TryGetFragment("temp", 2);
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

            ErpFragment? mipsFragment = resource.TryGetFragment("mips", 0) ?? resource.TryGetFragment("temp", 2);
            if (mipsFragment != null)
            {
                Frag2.ToFragment(mipsFragment);
            }
        }

        private UInt32 GetLinearSize(UInt32 width, UInt32 height)
        {
            switch (Fragment0.ImageType)
            {
                //case (ErpGfxSurfaceFormat)14: // gameparticles k_smoke; application
                case ErpGfxSurfaceFormat.ABGR8:
                    return width * height * 4;
                case ErpGfxSurfaceFormat.DXT1: // ferrari_wheel_sfc
                case ErpGfxSurfaceFormat.DXT1_SRGB: // ferrari_wheel_df, ferrari_paint
                case ErpGfxSurfaceFormat.ATI1: // gameparticles k_smoke
                    return width * height / 2;
                case ErpGfxSurfaceFormat.DXT5: // ferrari_sfc
                case ErpGfxSurfaceFormat.DXT5_SRGB: // ferrari_decal
                case ErpGfxSurfaceFormat.ATI2: // ferrari_wheel_nm
                case ErpGfxSurfaceFormat.BC6: // key0_2016; environment abu_dhabi tree_palm_06
                case ErpGfxSurfaceFormat.BC7:
                case ErpGfxSurfaceFormat.BC7_SRGB: // flow_boot splash_bg_image
                    return width * height;
                default:
                    return 0;
            }
        }
    }
}
