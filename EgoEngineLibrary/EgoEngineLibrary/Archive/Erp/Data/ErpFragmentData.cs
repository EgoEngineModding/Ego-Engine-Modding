using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgoEngineLibrary.Archive.Erp.Data
{
    public abstract class ErpFragmentData
    {
        public abstract void FromFragment(ErpFragment fragment);
        public abstract void ToFragment(ErpFragment fragment);
    }
}
