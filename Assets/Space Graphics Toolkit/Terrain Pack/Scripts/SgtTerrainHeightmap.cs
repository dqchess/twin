using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainHeightmap))]
	public class SgtTerrainHeightmap_Editor : SgtEditor<SgtTerrainHeightmap>
	{
		protected override void OnInspector()
		{
			var dirtyTerrain = false;

			BeginError(Any(t => t.Heightmap == null));
				DrawDefault("Heightmap", ref dirtyTerrain, "The heightmap texture using a cylindrical (equirectangular) projection.");
			EndError();
			DrawDefault("Encoding", ref dirtyTerrain, "The way the height data is stored in the texture.");
			BeginError(Any(t => t.DisplacementMin >= t.DisplacementMax));
				DrawDefault("DisplacementMin", ref dirtyTerrain, "The height displacement represented by alpha = 0.");
				DrawDefault("DisplacementMax", ref dirtyTerrain, "The height displacement represented by alpha = 255.");
			EndError();

			if (dirtyTerrain == true) DirtyEach(t => t.Rebuild());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component deforms the terrain using a heightmap.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainHeightmap")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Heightmap")]
	public class SgtTerrainHeightmap : SgtTerrainModifier
	{
		public enum EncodingType
		{
			Alpha,
			RedGreen
		}

		/// <summary>The heightmap texture using a cylindrical (equirectangular) projection.</summary>
		public Texture2D Heightmap;

		/// <summary>The way the height data is stored in the texture.</summary>
		public EncodingType Encoding = EncodingType.Alpha;

		/// <summary>The height displacement represented by alpha = 0.</summary>
		public double DisplacementMin = 0.0;

		/// <summary>The height displacement represented by alpha = 255.</summary>
		public double DisplacementMax = 0.1;

		protected virtual void OnEnable()
		{
			Rebuild();

			terrain.OnCalculateHeight += CalculateHeight;
		}

		protected virtual void OnDisable()
		{
			Rebuild();

			terrain.OnCalculateHeight -= CalculateHeight;
		}

		private void CalculateHeight(SgtVector3D localPosition, ref double height)
		{
			if (Heightmap != null)
			{
				var uv       = SgtHelper.CartesianToPolarUV((Vector3)localPosition);
				var color    = SampleBilinear(uv);
				var height01 = default(double);

				switch (Encoding)
				{
					case EncodingType.Alpha:
					{
						height01 = color.a;
					}
					break;

					case EncodingType.RedGreen:
					{
						height01 = (color.r * 255.0 + color.g) / 256.0;
					}
					break;
				}

				height += DisplacementMin + (DisplacementMax - DisplacementMin) * height01;
			}
		}

		private Color SampleBilinear(Vector2 uv)
		{
			return Heightmap.GetPixelBilinear(uv.x, uv.y);
		}
	}
}