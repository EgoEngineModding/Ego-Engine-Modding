using Microsoft.Toolkit.Diagnostics;
using SharpGLTF.Geometry.VertexTypes;
using System;
using System.Numerics;

using ENCODING = SharpGLTF.Schema2.EncodingType;

namespace EgoEngineLibrary.Formats.Pssg
{
    /// <summary>
    /// Defines a Vertex attribute with a material Color and four Texture Coordinates.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{_GetDebuggerDisplay(),nq}")]
    public struct VertexColor1Texture4 : IVertexMaterial, IEquatable<VertexColor1Texture4>
    {
        #region debug

        private string _GetDebuggerDisplay() => $"𝐂:{Color} 𝐔𝐕₀:{TexCoord0} 𝐔𝐕₁:{TexCoord1} 𝐔𝐕2:{TexCoord2} 𝐔𝐕3:{TexCoord3}";

        #endregion

        #region constructors

        public VertexColor1Texture4(Vector4 color, Vector2 tex0, Vector2 tex1, Vector2 tex2, Vector2 tex3)
        {
            Color = color;
            TexCoord0 = tex0;
            TexCoord1 = tex1;
            TexCoord2 = tex2;
            TexCoord3 = tex3;
        }

        public VertexColor1Texture4(IVertexMaterial src)
        {
            Guard.IsNotNull(src, nameof(src));

            this.Color = src.MaxColors > 0 ? src.GetColor(0) : Vector4.One;
            this.TexCoord0 = src.MaxTextCoords > 0 ? src.GetTexCoord(0) : Vector2.Zero;
            this.TexCoord1 = src.MaxTextCoords > 1 ? src.GetTexCoord(1) : Vector2.Zero;
            this.TexCoord2 = src.MaxTextCoords > 2 ? src.GetTexCoord(2) : Vector2.Zero;
            this.TexCoord3 = src.MaxTextCoords > 3 ? src.GetTexCoord(3) : Vector2.Zero;
        }

        public static implicit operator VertexColor1Texture4((Vector4 Color, Vector2 Tex0, Vector2 Tex1, Vector2 Tex2, Vector2 Tex3) tuple)
        {
            return new VertexColor1Texture4(tuple.Color, tuple.Tex0, tuple.Tex1, tuple.Tex2, tuple.Tex3);
        }

        #endregion

        #region data

        [VertexAttribute("COLOR_0", ENCODING.UNSIGNED_BYTE, true)]
        public Vector4 Color;

        [VertexAttribute("TEXCOORD_0")]
        public Vector2 TexCoord0;

        [VertexAttribute("TEXCOORD_1")]
        public Vector2 TexCoord1;

        [VertexAttribute("TEXCOORD_2")]
        public Vector2 TexCoord2;

        [VertexAttribute("TEXCOORD_3")]
        public Vector2 TexCoord3;

        public int MaxColors => 1;

        public int MaxTextCoords => 4;

        public override bool Equals(object? obj) { return obj is VertexColor1Texture4 other && AreEqual(this, other); }
        public bool Equals(VertexColor1Texture4 other) { return AreEqual(this, other); }
        public static bool operator ==(in VertexColor1Texture4 a, in VertexColor1Texture4 b) { return AreEqual(a, b); }
        public static bool operator !=(in VertexColor1Texture4 a, in VertexColor1Texture4 b) { return !AreEqual(a, b); }
        public static bool AreEqual(in VertexColor1Texture4 a, in VertexColor1Texture4 b)
        {
            return a.Color == b.Color &&
                a.TexCoord0 == b.TexCoord0 &&
                a.TexCoord1 == b.TexCoord1 &&
                a.TexCoord2 == b.TexCoord2 &&
                a.TexCoord3 == b.TexCoord3;
        }

        public override int GetHashCode() { return Color.GetHashCode() ^ TexCoord0.GetHashCode() ^ TexCoord1.GetHashCode(); }

        #endregion

        #region API

        void IVertexMaterial.SetColor(int setIndex, Vector4 color) { if (setIndex == 0) this.Color = color; }

        void IVertexMaterial.SetTexCoord(int setIndex, Vector2 coord)
        {
            if (setIndex == 0) this.TexCoord0 = coord;
            if (setIndex == 1) this.TexCoord1 = coord;
            if (setIndex == 2) this.TexCoord2 = coord;
            if (setIndex == 3) this.TexCoord3 = coord;
        }

        object? IVertexMaterial.GetCustomAttribute(string attributeName) { return null; }

        public Vector4 GetColor(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return Color;
        }

        public Vector2 GetTexCoord(int index)
        {
            switch (index)
            {
                case 0: return this.TexCoord0;
                case 1: return this.TexCoord1;
                case 2: return this.TexCoord2;
                case 3: return this.TexCoord3;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void Validate() { FragmentPreprocessors.ValidateVertexMaterial(this); }

        #endregion
    }

    /// <summary>
    /// Defines a Vertex attribute with a material Color and three Texture Coordinates.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{_GetDebuggerDisplay(),nq}")]
    public struct VertexColor1Texture3 : IVertexMaterial, IEquatable<VertexColor1Texture3>
    {
        #region debug

        private string _GetDebuggerDisplay() => $"𝐂:{Color} 𝐔𝐕₀:{TexCoord0} 𝐔𝐕₁:{TexCoord1} 𝐔𝐕2:{TexCoord2}";

        #endregion

        #region constructors

        public VertexColor1Texture3(Vector4 color, Vector2 tex0, Vector2 tex1, Vector2 tex2)
        {
            Color = color;
            TexCoord0 = tex0;
            TexCoord1 = tex1;
            TexCoord2 = tex2;
        }

        public VertexColor1Texture3(IVertexMaterial src)
        {
            Guard.IsNotNull(src, nameof(src));

            this.Color = src.MaxColors > 0 ? src.GetColor(0) : Vector4.One;
            this.TexCoord0 = src.MaxTextCoords > 0 ? src.GetTexCoord(0) : Vector2.Zero;
            this.TexCoord1 = src.MaxTextCoords > 1 ? src.GetTexCoord(1) : Vector2.Zero;
            this.TexCoord2 = src.MaxTextCoords > 2 ? src.GetTexCoord(2) : Vector2.Zero;
        }

        public static implicit operator VertexColor1Texture3((Vector4 Color, Vector2 Tex0, Vector2 Tex1, Vector2 Tex2) tuple)
        {
            return new VertexColor1Texture3(tuple.Color, tuple.Tex0, tuple.Tex1, tuple.Tex2);
        }

        #endregion

        #region data

        [VertexAttribute("COLOR_0", ENCODING.UNSIGNED_BYTE, true)]
        public Vector4 Color;

        [VertexAttribute("TEXCOORD_0")]
        public Vector2 TexCoord0;

        [VertexAttribute("TEXCOORD_1")]
        public Vector2 TexCoord1;

        [VertexAttribute("TEXCOORD_2")]
        public Vector2 TexCoord2;

        public int MaxColors => 1;

        public int MaxTextCoords => 3;

        public override bool Equals(object? obj) { return obj is VertexColor1Texture3 other && AreEqual(this, other); }
        public bool Equals(VertexColor1Texture3 other) { return AreEqual(this, other); }
        public static bool operator ==(in VertexColor1Texture3 a, in VertexColor1Texture3 b) { return AreEqual(a, b); }
        public static bool operator !=(in VertexColor1Texture3 a, in VertexColor1Texture3 b) { return !AreEqual(a, b); }
        public static bool AreEqual(in VertexColor1Texture3 a, in VertexColor1Texture3 b)
        {
            return a.Color == b.Color &&
                a.TexCoord0 == b.TexCoord0 &&
                a.TexCoord1 == b.TexCoord1 &&
                a.TexCoord2 == b.TexCoord2;
        }

        public override int GetHashCode() { return Color.GetHashCode() ^ TexCoord0.GetHashCode() ^ TexCoord1.GetHashCode(); }

        #endregion

        #region API

        void IVertexMaterial.SetColor(int setIndex, Vector4 color) { if (setIndex == 0) this.Color = color; }

        void IVertexMaterial.SetTexCoord(int setIndex, Vector2 coord)
        {
            if (setIndex == 0) this.TexCoord0 = coord;
            if (setIndex == 1) this.TexCoord1 = coord;
            if (setIndex == 2) this.TexCoord2 = coord;
        }

        object? IVertexMaterial.GetCustomAttribute(string attributeName) { return null; }

        public Vector4 GetColor(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return Color;
        }

        public Vector2 GetTexCoord(int index)
        {
            switch (index)
            {
                case 0: return this.TexCoord0;
                case 1: return this.TexCoord1;
                case 2: return this.TexCoord2;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public void Validate() { FragmentPreprocessors.ValidateVertexMaterial(this); }

        #endregion
    }

    internal static class FragmentPreprocessors
    {
        public static TvM? ValidateVertexMaterial<TvM>(TvM vertex)
            where TvM : struct, IVertexMaterial
        {
            var exclusiveOne = MathF.BitIncrement(1);
            for (int i = 0; i < vertex.MaxColors; ++i)
            {
                var c = vertex.GetColor(i);
                Guard.IsTrue(c._IsFinite(), $"Color{i}", "Values are not finite.");
                Guard.IsInRange(c.X, 0, exclusiveOne, $"Color{i}.R");
                Guard.IsInRange(c.Y, 0, exclusiveOne, $"Color{i}.G");
                Guard.IsInRange(c.Z, 0, exclusiveOne, $"Color{i}.B");
                Guard.IsInRange(c.W, 0, exclusiveOne, $"Color{i}.A");
            }

            for (int i = 0; i < vertex.MaxTextCoords; ++i)
            {
                var t = vertex.GetTexCoord(i);
                Guard.IsTrue(t._IsFinite(), $"TexCoord{i}", "Values are not finite.");
            }

            return vertex;
        }

        internal static bool _IsFinite(this float value)
        {
            return !(float.IsNaN(value) || float.IsInfinity(value));
        }

        internal static bool _IsFinite(this Vector2 v)
        {
            return v.X._IsFinite() && v.Y._IsFinite();
        }

        internal static bool _IsFinite(this Vector4 v)
        {
            return v.X._IsFinite() && v.Y._IsFinite() && v.Z._IsFinite() && v.W._IsFinite();
        }
    }
}
