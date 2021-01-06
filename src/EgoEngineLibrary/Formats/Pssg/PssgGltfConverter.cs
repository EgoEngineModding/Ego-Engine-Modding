using BCnEncoder.Decoder;
using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Graphics.Dds;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace EgoEngineLibrary.Formats.Pssg
{
    public abstract class PssgGltfConverter
	{
		protected static readonly byte[] blackImageBytes;
		protected static readonly byte[] grayImageBytes;
		protected static readonly byte[] whiteImageBytes;

		static PssgGltfConverter()
		{
			using (var image = new Image<Rgba32>(1, 1))
			using (var ms = new MemoryStream())
			{
				image[0, 0] = new Rgba32(0, 0, 0);
				image.SaveAsPng(ms);
				blackImageBytes = ms.ToArray();
			}

			using (var image = new Image<Rgba32>(1, 1))
			using (var ms = new MemoryStream())
			{
				image[0, 0] = new Rgba32(128, 128, 128);
				image.SaveAsPng(ms);
				grayImageBytes = ms.ToArray();
			}

			using (var image = new Image<Rgba32>(1, 1))
			using (var ms = new MemoryStream())
			{
				image[0, 0] = new Rgba32(255, 255, 255);
				image.SaveAsPng(ms);
				whiteImageBytes = ms.ToArray();
			}
		}

		protected static Matrix4x4 getTransform(byte[] buffer)
		{
			Matrix4x4 t = new Matrix4x4();
			MiscUtil.Conversion.BigEndianBitConverter bc = new MiscUtil.Conversion.BigEndianBitConverter();

			// Surely i've missed something and there's a way to loop through this? Please?
			int i = 0;
			t.M11 = bc.ToSingle(buffer, i); i += 4;
			t.M12 = bc.ToSingle(buffer, i); i += 4;
			t.M13 = bc.ToSingle(buffer, i); i += 4;
			t.M14 = bc.ToSingle(buffer, i); i += 4;

			t.M21 = bc.ToSingle(buffer, i); i += 4;
			t.M22 = bc.ToSingle(buffer, i); i += 4;
			t.M23 = bc.ToSingle(buffer, i); i += 4;
			t.M24 = bc.ToSingle(buffer, i); i += 4;

			t.M31 = bc.ToSingle(buffer, i); i += 4;
			t.M32 = bc.ToSingle(buffer, i); i += 4;
			t.M33 = bc.ToSingle(buffer, i); i += 4;
			t.M34 = bc.ToSingle(buffer, i); i += 4;

			t.M41 = bc.ToSingle(buffer, i); i += 4;
			t.M42 = bc.ToSingle(buffer, i); i += 4;
			t.M43 = bc.ToSingle(buffer, i); i += 4;
			t.M44 = bc.ToSingle(buffer, i); i += 4;

			return t;
		}

		protected static byte[] GetDiffuseTexture(PssgNode? sgNode, IEnumerable<PssgNode> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TDiffuseAlphaMap") ?? grayImageBytes;
		}

		protected static byte[] GetSpecularTexture(PssgNode? sgNode, IEnumerable<PssgNode> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TSpecularMap") ?? blackImageBytes;
		}

		protected static byte[] GetEmissiveTexture(PssgNode? sgNode, IEnumerable<PssgNode> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TEmissiveMap") ?? blackImageBytes;
		}

		protected static byte[] GetOcclusionTexture(PssgNode? sgNode, IEnumerable<PssgNode> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TOcclusionMap") ?? whiteImageBytes;
		}

		protected static byte[] GetNormalTexture(PssgNode? sgNode, IEnumerable<PssgNode> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TNormalMap") ?? blackImageBytes;
		}

		private static byte[]? GetTextureBytes(PssgNode? sgNode, IEnumerable<PssgNode> textureInputs, string texType)
		{
			try
			{
				if (sgNode is null)
					return null;

				foreach (var ti in textureInputs)
				{
					var paramId = ti.Attributes["parameterID"].GetValue<uint>();
					if (paramId >= sgNode.ChildNodes.Count)
						continue;

					var textureId = ti.Attributes["texture"].GetValue<string>();
					var refIndex = textureId.IndexOf('#');
					if (refIndex != 0) // this means we're refrencing another pssg file
						continue;
					textureId = textureId.Substring(1);

					var sidNode = sgNode.ChildNodes[(int)paramId];
					if (sidNode.HasAttribute("name") && sidNode.Attributes["name"].GetValue<string>().StartsWith(texType))
					{
						var textureNode = sgNode.File.FindNodes("TEXTURE", "id", textureId).FirstOrDefault();
						if (textureNode is null)
							continue;

						return GetTextureBytes(textureNode);
					}
				}

				return null;
			}
			catch
			{
				return null;
			}
		}

		private static byte[] GetTextureBytes(PssgNode textureNode)
		{
			using (var ms = new MemoryStream())
			{
				var dds = textureNode.ToDdsFile(false);
				dds.Write(ms, -1);
				ms.Seek(0, SeekOrigin.Begin);

				var bcDecode = new BcDecoder();
				var img = bcDecode.Decode(ms);
				ms.Seek(0, SeekOrigin.Begin);

				img.SaveAsPng(ms);
				return ms.ToArray();
			}
		}
	}
}
