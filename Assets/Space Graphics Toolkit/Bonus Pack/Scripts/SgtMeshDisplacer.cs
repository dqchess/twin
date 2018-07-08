using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtMeshDisplacer))]
	public class SgtMeshDisplacer_Editor : SgtEditor<SgtMeshDisplacer>
	{
		protected override void OnInspector()
		{
			var rebuildMesh = false;

			BeginError(Any(t => t.OriginalMesh == null));
				DrawDefault("OriginalMesh", ref rebuildMesh, "The original mesh we want to displace.");
			EndError();
			BeginError(Any(t => t.Heightmap == null));
				DrawDefault("Heightmap", ref rebuildMesh, "The height map texture used to displace the mesh (Height must be stored in alpha channel).");
			EndError();
			DrawDefault("Encoding", ref rebuildMesh, "The way the height data is stored in the texture.");
			BeginError(Any(t => t.InnerRadius == t.OuterRadius));
				DrawDefault("InnerRadius", ref rebuildMesh, "The mesh radius represented by a 0 alpha value.");
				DrawDefault("OuterRadius", ref rebuildMesh, "The mesh radius represented by a 255 alpha value.");
			EndError();

			if (rebuildMesh == true) DirtyEach(t => t.RebuildMesh());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component converts a normal spherical mesh into one displaced by a heightmap.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtMeshDisplacer")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Mesh Displacer")]
	public class SgtMeshDisplacer : MonoBehaviour
	{
		public enum EncodingType
		{
			Alpha,
			RedGreen
		}

		/// <summary>The original mesh we want to displace.</summary>
		public Mesh OriginalMesh;

		/// <summary>The height map texture used to displace the mesh (Height must be stored in alpha channel).</summary>
		public Texture2D Heightmap;

		/// <summary>The way the height data is stored in the texture.</summary>
		public EncodingType Encoding = EncodingType.Alpha;

		/// <summary>The mesh radius represented by a 0 alpha value.</summary>
		public float InnerRadius = 0.9f;

		/// <summary>The mesh radius represented by a 255 alpha value.</summary>
		public float OuterRadius = 1.1f;

		[System.NonSerialized]
		private Mesh displacedMesh;

		[System.NonSerialized]
		private MeshFilter meshFilter;

		// Call this if you've made any changes from code and need the mesh to get rebuilt
		[ContextMenu("Rebuild Mesh")]
		public void RebuildMesh()
		{
			displacedMesh = SgtHelper.Destroy(displacedMesh);

			if (OriginalMesh != null && Heightmap != null)
			{
	#if UNITY_EDITOR
				SgtHelper.MakeTextureReadable(Heightmap);
	#endif
				// Duplicate original
				displacedMesh = Instantiate(OriginalMesh);
	#if UNITY_EDITOR
				displacedMesh.hideFlags = HideFlags.DontSave;
	#endif
				displacedMesh.name = OriginalMesh.name + " (Displaced)";

				// Displace vertices
				var positions = OriginalMesh.vertices;

				for (var i = 0; i < positions.Length; i++)
				{
					var direction = positions[i].normalized;

					positions[i] = direction * GetSurfaceHeightLocal(direction);
				}

				displacedMesh.vertices = positions;

				displacedMesh.RecalculateBounds();
			}

			// Apply displaced mesh
			if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

			meshFilter.sharedMesh = displacedMesh;
		}

		protected virtual void OnEnable()
		{
			if (displacedMesh != null)
			{
				meshFilter.sharedMesh = displacedMesh;
			}
		}

		protected virtual void Start()
		{
			if (OriginalMesh == null)
			{
				if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

				OriginalMesh = meshFilter.sharedMesh;
			}

			if (OriginalMesh != null)
			{
				RebuildMesh();
			}
		}

		protected virtual void OnDisable()
		{
			if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

			meshFilter.sharedMesh = OriginalMesh;
		}

	#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, InnerRadius);
			Gizmos.DrawWireSphere(Vector3.zero, OuterRadius);
		}
	#endif

		// This will return the local terrain height at the given local position
		public float GetSurfaceHeightLocal(Vector3 localPosition)
		{
			var uv       = SgtHelper.CartesianToPolarUV(localPosition);
			var color    = SampleBilinear(uv);
			var height01 = default(float);

			switch (Encoding)
			{
				case EncodingType.Alpha:
				{
					height01 = color.a;
				}
				break;

				case EncodingType.RedGreen:
				{
					height01 = (color.r * 255.0f + color.g) / 256.0f;
				}
				break;
			}

			return Mathf.Lerp(InnerRadius, OuterRadius, height01);
		}

		private Color SampleBilinear(Vector2 uv)
		{
			return Heightmap.GetPixelBilinear(uv.x, uv.y);
		}
	}
}