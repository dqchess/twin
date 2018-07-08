using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProminence))]
	public class SgtProminence_Editor : SgtEditor<SgtProminence>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateMesh     = false;
			var updatePlanes   = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the prominence material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The main texture of the prominence.");
			EndError();
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the prominence, giving you fine control over the render order."); // Updated automatically
			DrawDefault("Seed", ref updatePlanes, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.PlaneCount < 1));
				DrawDefault("PlaneCount", ref updatePlanes, "The amount of planes used to build the prominence.");
			EndError();
			BeginError(Any(t => t.PlaneDetail < 3));
				DrawDefault("PlaneDetail", ref updateMesh, "The amount of quads used to build each plane.");
			EndError();
			BeginError(Any(t => t.RadiusMin < 0.0f || t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMin", ref updateMesh, "The inner radius of the prominence planes in local coordinates.");
			EndError();
			BeginError(Any(t => t.RadiusMax < 0.0f || t.RadiusMin >= t.RadiusMax));
				DrawDefault("RadiusMax", ref updateMesh, "The outer radius of the prominence planes in local coordinates.");
			EndError();

			Separator();

			DrawDefault("FadeEdge", ref updateMaterial, "Should the plane fade out when it's viewed edge-on?");

			if (Any(t => t.FadeEdge == true))
			{
				BeginIndent();
					BeginError(Any(t => t.FadePower < 0.0f));
						DrawDefault("FadePower", ref updateMaterial, "How sharp the transition between visible and invisible is.");
					EndError();
				EndIndent();
			}

			DrawDefault("ClipNear", ref updateMaterial, "Should the plane fade out when it's in front of the star?");

			if (Any(t => t.ClipNear == true))
			{
				BeginIndent();
					BeginError(Any(t => t.ClipPower < 0.0f));
						DrawDefault("ClipPower", ref updateMaterial, "How sharp the transition between visible and invisible is.");
					EndError();
				EndIndent();
			}

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateMesh     == true) DirtyEach(t => t.UpdateMesh    ());
			if (updatePlanes   == true) DirtyEach(t => t.UpdateModels  ());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a halo disc around a star.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProminence")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Prominence")]
	public class SgtProminence : MonoBehaviour
	{
		/// <summary>The main texture of the prominence.</summary>
		public Texture MainTex;

		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the prominence material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The amount of planes used to build the prominence.</summary>
		public int PlaneCount = 8;

		/// <summary>The amount of quads used to build each plane.</summary>
		public int PlaneDetail = 10;

		/// <summary>The inner radius of the prominence planes in local coordinates.</summary>
		public float RadiusMin = 1.0f;

		/// <summary>The outer radius of the prominence planes in local coordinates.</summary>
		public float RadiusMax = 2.0f;

		/// <summary>Should the plane fade out when it's viewed edge-on?</summary>
		public bool FadeEdge;

		/// <summary>How sharp the transition between visible and invisible is.</summary>
		public float FadePower = 2.0f;

		/// <summary>Should the plane fade out when it's in front of the star?</summary>
		public bool ClipNear;

		/// <summary>How sharp the transition between visible and invisible is.</summary>
		public float ClipPower = 2.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the prominence, giving you fine control over the render order.</summary>
		public float CameraOffset;

		// The planes used to make up this prominence
		[SerializeField]
		private List<SgtProminenceModel> models;

		// The material applied to all segments
		[System.NonSerialized]
		private Material material;

		// The mesh applied to all segments
		[System.NonSerialized]
		private Mesh mesh;

		public float Width
		{
			get
			{
				return RadiusMax - RadiusMin;
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Prominence (Generated)", SgtHelper.ShaderNamePrefix + "Prominence");

				if (models != null)
				{
					for (var i = models.Count - 1; i >= 0; i--)
					{
						var model = models[i];

						if (model != null)
						{
							model.SetMaterial(material);
						}
					}
				}
			}

			var color = SgtHelper.Premultiply(SgtHelper.Brighten(Color, Brightness));

			material.renderQueue = RenderQueue;

			material.SetTexture("_MainTex", MainTex);
			material.SetColor("_Color", color);
			material.SetVector("_WorldPosition", transform.position);

			SgtHelper.SetTempMaterial(material);

			if (FadeEdge == true)
			{
				SgtHelper.EnableKeyword("SGT_A");

				material.SetFloat("_FadePower", FadePower);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A");
			}

			if (ClipNear == true)
			{
				SgtHelper.EnableKeyword("SGT_B");

				material.SetFloat("_ClipPower", ClipPower);
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B");
			}
		}

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (mesh == null)
			{
				mesh = SgtHelper.CreateTempMesh("Plane");

				if (models != null)
				{
					for (var i = models.Count - 1; i >= 0; i--)
					{
						var model = models[i];

						if (model != null)
						{
							model.SetMesh(mesh);
						}
					}
				}
			}
			else
			{
				mesh.Clear(false);
			}

			if (PlaneDetail >= 2)
			{
				var detail    = Mathf.Min(PlaneDetail, SgtHelper.QuadsPerMesh / 2); // Limit the amount of vertices that get made
				var positions = new Vector3[detail * 2 + 2];
				var normals   = new Vector3[detail * 2 + 2];
				var coords1   = new Vector2[detail * 2 + 2];
				var coords2   = new Vector2[detail * 2 + 2];
				var indices   = new int[detail * 6];
				var angleStep = SgtHelper.Divide(Mathf.PI * 2.0f, detail);
				var coordStep = SgtHelper.Reciprocal(detail);

				for (var i = 0; i <= detail; i++)
				{
					var coord = coordStep * i;
					var angle = angleStep * i;
					var sin   = Mathf.Sin(angle);
					var cos   = Mathf.Cos(angle);
					var offV  = i * 2;

					positions[offV + 0] = new Vector3(sin * RadiusMin, 0.0f, cos * RadiusMin);
					positions[offV + 1] = new Vector3(sin * RadiusMax, 0.0f, cos * RadiusMax);

					normals[offV + 0] = Vector3.up;
					normals[offV + 1] = Vector3.up;

					coords1[offV + 0] = new Vector2(0.0f, coord * RadiusMin);
					coords1[offV + 1] = new Vector2(1.0f, coord * RadiusMax);

					coords2[offV + 0] = new Vector2(RadiusMin, 0.0f);
					coords2[offV + 1] = new Vector2(RadiusMax, 0.0f);
				}

				for (var i = 0; i < detail; i++)
				{
					var offV = i * 2;
					var offI = i * 6;

					indices[offI + 0] = offV + 0;
					indices[offI + 1] = offV + 1;
					indices[offI + 2] = offV + 2;
					indices[offI + 3] = offV + 3;
					indices[offI + 4] = offV + 2;
					indices[offI + 5] = offV + 1;
				}

				mesh.vertices  = positions;
				mesh.normals   = normals;
				mesh.uv        = coords1;
				mesh.uv2       = coords2;
				mesh.triangles = indices;
			}
		}

		[ContextMenu("Update Models")]
		public void UpdateModels()
		{
			SgtHelper.BeginRandomSeed(Seed);
			{
				for (var i = 0; i < PlaneCount; i++)
				{
					var model = SgtHelper.GetIndex(ref models, i);

					if (model == null)
					{
						model = models[i] = SgtProminenceModel.Create(this);
					}

					model.SetRotation(Random.rotationUniform);
					model.SetMaterial(material);
					model.SetMesh(mesh);
				}
			}
			SgtHelper.EndRandomSeed();

			// Remove any excess
			if (models != null)
			{
				var min = Mathf.Max(0, PlaneCount);

				for (var i = models.Count - 1; i >= min; i--)
				{
					SgtProminenceModel.Pool(models[i]);

					models.RemoveAt(i);
				}
			}
		}

		public static SgtProminence Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtProminence Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Prominence", layer, parent, localPosition, localRotation, localScale);
			var prominence = gameObject.AddComponent<SgtProminence>();

			return prominence;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Prominence", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var prominence = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(prominence);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.gameObject.SetActive(true);
					}
				}
			}

			UpdateMaterial();
			UpdateMesh();
			UpdateModels();
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.gameObject.SetActive(false);
					}
				}
			}
		}

		protected virtual void OnDestroy()
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					SgtProminenceModel.MarkForDestruction(models[i]);
				}
			}

			if (mesh != null)
			{
				mesh.Clear(false);

				SgtObjectPool<Mesh>.Add(mesh);
			}

			SgtHelper.Destroy(material);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, RadiusMin);

			Gizmos.DrawWireSphere(Vector3.zero, RadiusMax);
		}
#endif

		private void CameraPreCull(Camera camera)
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.Revert();
						{
							if (CameraOffset != 0.0f)
							{
								var direction = transform.position - camera.transform.position;

								model.transform.position += direction.normalized * CameraOffset;
							}
						}
						model.Save(camera);
					}
				}
			}
		}

		private void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				material.SetVector("_WorldPosition", transform.position);
			}

			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.Restore(camera);
					}
				}
			}
		}
	}
}