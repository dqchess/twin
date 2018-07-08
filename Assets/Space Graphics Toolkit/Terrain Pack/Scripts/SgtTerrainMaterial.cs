using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainMaterial))]
	public class SgtTerrainMaterial_Editor : SgtEditor<SgtTerrainMaterial>
	{
		protected override void OnInspector()
		{
			var dirtyTerrain = false;

			BeginError(Any(t => t.Material == null));
				DrawDefault("Material", ref dirtyTerrain, "The material that will be assigned.");
			EndError();
			DrawDefault("AllSides", ref dirtyTerrain, "Apply this material to all sides?");
			if (Any(t => t.AllSides == false))
			{
				BeginIndent();
					DrawDefault("RequiredSide", ref dirtyTerrain, "The side this material will be applied to.");
				EndIndent();
			}
			BeginError(Any(t => t.LevelMin < 0 || t.LevelMin > t.LevelMax));
				DrawDefault("LevelMin", ref dirtyTerrain, "The minimum LOD level this material will be applied to.");
				DrawDefault("LevelMax", ref dirtyTerrain, "The maximum LOD level this material will be applied to.");
			EndError();

			if (dirtyTerrain == true) DirtyEach(t => t.Rebuild());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to specify a particular material to use based on the side or level.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainMaterial")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Material")]
	public class SgtTerrainMaterial : SgtTerrainModifier
	{
		/// <summary>The material that will be assigned.</summary>
		public Material Material;

		/// <summary>Apply this material to all sides?</summary>
		public bool AllSides = true;

		/// <summary>The side this material will be applied to.</summary>
		public CubemapFace RequiredSide;

		/// <summary>The minimum LOD level this material will be applied to.</summary>
		public int LevelMin = 5;

		/// <summary>The maximum LOD level this material will be applied to.</summary>
		public int LevelMax = 5;

		protected virtual void OnEnable()
		{
			Rebuild();

			terrain.OnCalculateMaterial += CalculateMaterial;
		}

		protected virtual void OnDisable()
		{
			Rebuild();

			terrain.OnCalculateMaterial -= CalculateMaterial;
		}

		private void CalculateMaterial(SgtTerrainFace face, ref Material material)
		{
			if (face.Depth >= LevelMin && face.Depth <= LevelMax)
			{
				if (AllSides == true || face.Side == RequiredSide)
				{
					material = Material;
				}
			}
		}
	}
}