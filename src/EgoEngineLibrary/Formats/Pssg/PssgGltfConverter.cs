using BCnEncoder.Decoder;
using EgoEngineLibrary.Graphics;
using EgoEngineLibrary.Graphics.Dds;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
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
		protected record PrimitiveData(PssgNode Node, MaterialBuilder Material, bool CreatedNewMaterial, RenderDataSourceReader Rds);
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

		protected static Matrix4x4 GetTransform(PssgNode sceneNode)
		{
			var transformNode = sceneNode.FindNodes("TRANSFORM").First();
			return GetTransform(transformNode.Value);
		}
		private static Matrix4x4 GetTransform(byte[] buffer)
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

			var color = rds.GetColor(index);
			vb.Material.Color = new Vector4(
				((color >> 8) & 0xFF) / (float)byte.MaxValue,
				((color >> 16) & 0xFF) / (float)byte.MaxValue,
				((color >> 24) & 0xFF) / (float)byte.MaxValue,
				((color >> 0) & 0xFF) / (float)byte.MaxValue);

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
