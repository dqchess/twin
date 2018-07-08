using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainCubeMaterials))]
	public class SgtTerrainCubeMaterials_Editor : SgtEditor<SgtTerrainCubeMaterials>
	{
		protected override void OnInspector()
		{
			var updateMaterials = false;

			BeginError(Any(t => t.NegativeX == null));
				DrawDefault("NegativeX", ref updateMaterials);
			EndError();
			BeginError(Any(t => t.NegativeY == null));
				DrawDefault("NegativeY", ref updateMaterials);
			EndError();
			BeginError(Any(t => t.NegativeZ == null));
				DrawDefault("NegativeZ", ref updateMaterials);
			EndError();
			BeginError(Any(t => t.PositiveX == null));
				DrawDefault("PositiveX", ref updateMaterials);
			EndError();
			BeginError(Any(t => t.PositiveY == null));
				DrawDefault("PositiveY", ref updateMaterials);
			EndError();
			BeginError(Any(t => t.PositiveZ == null));
				DrawDefault("PositiveZ", ref updateMaterials);
			EndError();

			if (updateMaterials == true) DirtyEach(t => t.UpdateRenderers());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you apply one terrain material for each side of the cube.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainCubeMaterials")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Cube Materials")]
	public class SgtTerrainCubeMaterials : SgtTerrainModifier
	{
		/// <summary>The material applied to all terrain faces on the -X side.</summary>
		public Material NegativeX;

		/// <summary>The material applied to all terrain faces on the -Y side.</summary>
		public Material NegativeY;

		/// <summary>The material applied to all terrain faces on the -Z side.</summary>
		public Material NegativeZ;

		/// <summary>The material applied to all terrain faces on the +X side.</summary>
		public Material PositiveX;

		/// <summary>The material applied to all terrain faces on the +Y side.</summary>
		public Material PositiveY;

		/// <summary>The material applied to all terrain faces on the +Z side.</summary>
		public Material PositiveZ;

		protected virtual void OnEnable()
		{
			UpdateRenderers();

			terrain.OnCalculateMaterial += CalculateMaterial;
		}

		protected virtual void OnDisable()
		{
			UpdateRenderers();

			terrain.OnCalculateMaterial -= CalculateMaterial;
		}

		private void CalculateMaterial(SgtTerrainFace face, ref Material material)
		{
			switch (face.Side)
			{
				case CubemapFace.NegativeX: material = NegativeX; break;
				case CubemapFace.NegativeY: material = NegativeY; break;
				case CubemapFace.NegativeZ: material = NegativeZ; break;
				case CubemapFace.PositiveX: material = PositiveX; break;
				case CubemapFace.PositiveY: material = PositiveY; break;
				case CubemapFace.PositiveZ: material = PositiveZ; break;
			}
		}
	}
}