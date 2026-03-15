using BCnEncoder.Decoder;
using EgoEngineLibrary.Graphics;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

using EgoEngineLibrary.Conversion;
using EgoEngineLibrary.Graphics.Pssg;
using EgoEngineLibrary.Graphics.Pssg.Elements;

namespace EgoEngineLibrary.Formats.Pssg
{
    public abstract class PssgGltfConverter
	{
		protected record PrimitiveData(PssgElement Element, MaterialBuilder Material, bool CreatedNewMaterial, RenderDataSourceReader Rds);
		protected abstract class PssgModelReaderState
		{
			public bool IsF1 { get; set; }

			public Dictionary<string, MaterialBuilder> ShaderMaterialMap { get; }

			public PssgModelReaderState()
			{
				ShaderMaterialMap = new Dictionary<string, MaterialBuilder>();
			}
		}

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

		protected static Matrix4x4 GetTransform(PssgElement sceneElement)
		{
			var transformNode = sceneElement.FindElements("TRANSFORM").First();
			return GetTransform(transformNode.Value);
		}
		private static Matrix4x4 GetTransform(byte[] buffer)
		{
			Matrix4x4 t = new Matrix4x4();
			BigEndianBitConverter bc = new BigEndianBitConverter();

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

		protected static IMeshBuilder<MaterialBuilder> CreateMeshBuilder(string name, int texCoordSets)
		{
			return texCoordSets switch
			{
				0 => new MeshBuilder<VertexPositionNormal, VertexColor1, VertexEmpty>(name),
				1 => new MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexEmpty>(name),
				2 => new MeshBuilder<VertexPositionNormal, VertexColor1Texture2, VertexEmpty>(name),
				3 => new MeshBuilder<VertexPositionNormal, VertexColor1Texture3, VertexEmpty>(name),
				_ => new MeshBuilder<VertexPositionNormal, VertexColor1Texture4, VertexEmpty>(name)
			};
		}

		protected static IVertexBuilder CreateVertexBuilder(RenderDataSourceReader rds, uint index, PssgModelReaderState state)
		{
			var vb = new VertexBuilder<VertexPositionNormal, VertexColor1Texture4, VertexEmpty>();
			vb.Geometry.Position = rds.GetPosition(index);
			vb.Geometry.Normal = rds.GetNormal(index);
			// Sometimes the normal would be NaNs, and the tangent/binormal zeros
			// not sure what to do in this case so I just leave out the tangent
			//vb.Geometry.Tangent = rds.GetTangent(index);

			if (state.IsF1)
			{
				// F1 puts diffuse in 1 and spec occ in 0, swap it
				vb.Material.TexCoord0 = rds.GetTexCoord(index, 1);
				vb.Material.TexCoord1 = rds.GetTexCoord(index, 0);
			}
			else
			{
				vb.Material.TexCoord0 = rds.GetTexCoord(index, 0);
				vb.Material.TexCoord1 = rds.GetTexCoord(index, 1);
			}
			vb.Material.TexCoord2 = rds.GetTexCoord(index, 2);
			vb.Material.TexCoord3 = rds.GetTexCoord(index, 3);

			vb.Material.Color = rds.GetColor(index);

			return vb;
		}

		protected static MaterialBuilder CreateMaterialBuilder(string shaderInstanceId, PssgModelReaderState state, out bool createdNew)
		{
			if (state.ShaderMaterialMap.TryGetValue(shaderInstanceId, out MaterialBuilder? mat))
			{
				createdNew = false;
				return mat;
			}

			mat = new MaterialBuilder(shaderInstanceId);
			state.ShaderMaterialMap.Add(shaderInstanceId, mat);

			createdNew = true;
			return mat;
		}

		protected static byte[] GetDiffuseTexture(PssgElement? sgNode, IEnumerable<PssgElement> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TDiffuseAlphaMap") ?? grayImageBytes;
		}
		protected static byte[] GetSpecularTexture(PssgElement? sgNode, IEnumerable<PssgElement> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TSpecularMap") ?? blackImageBytes;
		}
		protected static byte[] GetEmissiveTexture(PssgElement? sgNode, IEnumerable<PssgElement> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TEmissiveMap") ?? blackImageBytes;
		}
		protected static byte[] GetOcclusionTexture(PssgElement? sgNode, IEnumerable<PssgElement> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TOcclusionMap") ?? whiteImageBytes;
		}
		protected static byte[] GetNormalTexture(PssgElement? sgNode, IEnumerable<PssgElement> textureInputs)
		{
			return GetTextureBytes(sgNode, textureInputs, "TNormalMap") ?? blackImageBytes;
		}
		private static byte[]? GetTextureBytes(PssgElement? sgNode, IEnumerable<PssgElement> textureInputs, string texType)
		{
			try
			{
				if (sgNode is null)
					return null;

				foreach (var ti in textureInputs)
				{
					var paramId = ti.Attributes["parameterID"].GetValue<uint>();
					if (paramId >= sgNode.ChildElements.Count)
						continue;

					var textureId = ti.Attributes["texture"].GetValue<string>();
					var refIndex = textureId.IndexOf('#');
					if (refIndex != 0) // this means we're refrencing another pssg file
						continue;
					textureId = textureId.Substring(1);

					var sidNode = sgNode.ChildElements[(int)paramId];
					if (sidNode.HasAttribute("name") && sidNode.Attributes["name"].GetValue<string>().StartsWith(texType))
					{
						var textureNode = sgNode.File.FindElements("TEXTURE", "id", textureId).FirstOrDefault();
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
		private static byte[] GetTextureBytes(PssgElement textureElement)
		{
			using (var ms = new MemoryStream())
			{
				var dds = ((PssgTexture)textureElement).ToDdsFile();
				dds.Write(ms);

				ms.Seek(0, SeekOrigin.Begin);
                var bcDds = BCnEncoder.Shared.ImageFiles.DdsFile.Load(ms);

                var bcDecode = new BcDecoder();
                var pixels = new Rgba32[dds.header.width * dds.header.height];
                bcDecode.DecodeDdsToPixels<Rgba32>(bcDds, pixels);

                using var img = Image.WrapMemory<Rgba32>(pixels, (int)dds.header.width, (int)dds.header.height);
				ms.Seek(0, SeekOrigin.Begin);
				img.SaveAsPng(ms);
				return ms.ToArray();
			}
		}
	}
}
