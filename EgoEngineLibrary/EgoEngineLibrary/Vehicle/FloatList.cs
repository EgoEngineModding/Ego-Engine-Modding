namespace EgoEngineLibrary.Vehicle
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public struct FloatList
    {
        public int count;
        public float step;
        public float[] items;

        public static bool operator ==(FloatList x, FloatList y)
        {
            if (x.count == y.count && x.step == y.step && Enumerable.SequenceEqual(x.items, y.items))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool operator !=(FloatList x, FloatList y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            return obj is FloatList && this == (FloatList)obj;
        }

        public override int GetHashCode()
        {
            return count.GetHashCode() ^ step.GetHashCode() ^ items.GetHashCode();
        }
    }
}
