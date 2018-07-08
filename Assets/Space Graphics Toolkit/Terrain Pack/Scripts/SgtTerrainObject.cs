using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainObject))]
	public class SgtTerrainObject_Editor : SgtEditor<SgtTerrainObject>
	{
		protected override void OnInspector()
		{
			DrawDefault("Pool", "Can this object be pooled?");
			DrawDefault("ScaleMin", "The minimum scale this prefab is multiplied by when spawned.");
			DrawDefault("ScaleMax", "The maximum scale this prefab is multiplied by when spawned.");
			DrawDefault("AlignToNormal", "How far from the center the height samples are taken to align to the surface normal in world coordinates (0 = no alignment).");

			Separator();

			BeginDisabled();
				DrawDefault("Prefab", "The prefab this was instantiated from.");
			EndDisabled();
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component can be added to prefabs to make them spawnable with the SgtTerrainSpawner.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainObject")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Object")]
	public class SgtTerrainObject : MonoBehaviour
	{
		/// <summary>Called when this object is spawned (if pooling is enabled).</summary>
		public System.Action OnSpawn;

		/// <summary>Called when this object is despawned (if pooling is enabled).</summary>
		public System.Action OnDespawn;

		/// <summary>Can this object be pooled?</summary>
		public bool Pool;

		/// <summary>The minimum scale this prefab is multiplied by when spawned.</summary>
		public float ScaleMin = 1.0f;

		/// <summary>The maximum scale this prefab is multiplied by when spawned.</summary>
		public float ScaleMax = 1.1f;

		/// <summary>How far from the center the height samples are taken to align to the surface normal in world coordinates (0 = no alignment).</summary>
		public float AlignToNormal;

		/// <summary>The prefab this was instantiated from.</summary>
		public SgtTerrainObject Prefab;

		public void Spawn(SgtTerrain terrain, SgtTerrainFace face, SgtVector3D localPoint)
		{
			if (OnSpawn != null) OnSpawn();

			transform.SetParent(face.transform, false);

			// Snap to surface
			localPoint = terrain.GetLocalPoint(localPoint);

			// Rotate up
			var up = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f) * Vector3.up;

			// Spawn on surface
			var twist = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

			transform.localPosition = (Vector3)localPoint;
			transform.localRotation = Quaternion.FromToRotation(up, terrain.transform.TransformDirection(transform.localPosition)) * twist;
			transform.localScale    = Prefab.transform.localScale * Random.Range(ScaleMin, ScaleMax);
			//transform.rotation   = Quaternion.FromToRotation(up, terrain.transform.TransformDirection(localPosition));
		
			if (AlignToNormal != 0.0f)
			{
				var localRight   = transform.right   * AlignToNormal;
				var localForward = transform.forward * AlignToNormal;
				var localNormal  = terrain.GetLocalNormal(localPoint, new SgtVector3D(localRight), new SgtVector3D(localForward));

				transform.rotation = Quaternion.FromToRotation(up, (Vector3)localNormal) * twist;
			}
		}

		public void Despawn()
		{
			if (OnDespawn != null) OnDespawn();

			if (Pool == true)
			{
				SgtComponentPool<SgtTerrainObject>.Add(this);
			}
			else
			{
				SgtHelper.Destroy(gameObject);
			}
		}
	}
}