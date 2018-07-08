using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCloudsphere))]
	public class SgtCloudsphere_Editor : SgtEditor<SgtCloudsphere>
	{
		protected override void OnInspector()
		{
			var updateMaterial = false;
			var updateModel   = false;

			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness <= 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the cloudsphere material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The cube map applied to the cloudsphere surface.");
			EndError();
			BeginError(Any(t => t.DepthTex == null));
				DrawDefault("DepthTex", ref updateMaterial, "The look up table associating optical depth with cloud color. The left side is used when the depth is thin (e.g. edge of the cloudsphere when looking from space). The right side is used when the depth is thick (e.g. center of the cloudsphere when looking from space).");
			EndError();
			BeginError(Any(t => t.Radius < 0.0f));
				DrawDefault("Radius", ref updateModel, "This allows you to set the radius of the cloudsphere in local space.");
			EndError();
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the cloudsphere, giving you fine control over the render order."); // Updated automatically

			Separator();

			DrawDefault("Lit", ref updateMaterial, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(t => t.Lit == true))
			{
				BeginIndent();
					BeginError(Any(t => t.LightingTex == null));
						DrawDefault("LightingTex", ref updateMaterial, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
				EndIndent();
			}

			Separator();

			DrawDefault("Near", ref updateMaterial, "Enable this if you want the cloudsphere to fade out as the camera approaches.");

			if (Any(t => t.Near == true))
			{
				BeginIndent();
					BeginError(Any(t => t.NearTex == null));
						DrawDefault("NearTex", ref updateMaterial, "The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.");
					EndError();
					BeginError(Any(t => t.NearDistance <= 0.0f));
						DrawDefault("NearDistance", ref updateMaterial, "The distance the fading begins from in world space.");
					EndError();
				EndIndent();
			}

			Separator();
			
			BeginError(Any(t => t.Mesh == null));
				DrawDefault("Mesh", ref updateModel, "This allows you to set the mesh used to render the cloudsphere. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.MeshRadius <= 0.0f));
				DrawDefault("MeshRadius", ref updateModel, "This allows you to set the radius of the Mesh. If this is incorrectly set then the cloudsphere will render incorrectly.");
			EndError();

			if (Any(t => t.DepthTex == null && t.GetComponent<SgtCloudsphereDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCloudsphereDepthTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtCloudsphereLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCloudsphereLightingTex>(t.gameObject));
				}
			}

			if (Any(t => t.Near == true && t.NearTex == null && t.GetComponent<SgtCloudsphereNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCloudsphereNearTex>(t.gameObject));
				}
			}

			if (Any(t => SetMeshAndMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & Mesh Radius") == true)
				{
					Each(t => SetMeshAndMeshRadius(t, true));
				}
			}

			if (updateMaterial == true) DirtyEach(t => t.UpdateMaterial());
			if (updateModel    == true) DirtyEach(t => t.UpdateModel   ());
		}

		private bool SetMeshAndMeshRadius(SgtCloudsphere cloudsphere, bool apply)
		{
			if (cloudsphere.Mesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						cloudsphere.Mesh       = mesh;
						cloudsphere.MeshRadius = SgtHelper.GetMeshRadius(mesh);

						cloudsphere.UpdateMaterial();
						cloudsphere.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a sphere around a planet with a cloud cubemap.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCloudsphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Cloudsphere")]
	public class SgtCloudsphere : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the cloudsphere material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to set the mesh used to render the cloudsphere. This should be a sphere.</summary>
		public Mesh Mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the cloudsphere will render incorrectly.</summary>
		public float MeshRadius = 1.0f;

		/// <summary>This allows you to set the radius of the cloudsphere in local space.</summary>
		public float Radius = 1.5f;

		/// <summary>The cube map applied to the cloudsphere surface.</summary>
		public Cubemap MainTex;

		/// <summary>The look up table associating optical depth with cloud color. The left side is used when the depth is thin (e.g. edge of the cloudsphere when looking from space). The right side is used when the depth is thick (e.g. center of the cloudsphere when looking from space).</summary>
		public Texture2D DepthTex;

		/// <summary>Enable this if you want the cloudsphere to fade out as the camera approaches.</summary>
		public bool Near;

		/// <summary>The lookup table used to calculate the fade opacity based on distance, where the left side is used when the camera is close, and the right side is used when the camera is far.</summary>
		public Texture NearTex;

		/// <summary>The distance the fading begins from in world space.</summary>
		public float NearDistance = 1.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the cloudsphere, giving you fine control over the render order.</summary>
		public float CameraOffset;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		/// <summary>The model used to render the cloudsphere.</summary>
		[SerializeField]
		private SgtCloudsphereModel model;

		/// <summary>The material applied to the model.</summary>
		[System.NonSerialized]
		private Material material;

		/// <summary>This is used to optimize shader calculations.</summary>
		[System.NonSerialized]
		private bool renderedThisFrame;

		public void UpdateDepthTex()
		{
			if (material != null)
			{
				material.SetTexture("_DepthTex", DepthTex);
			}
		}

		public void UpdateNearTex()
		{
			if (material != null)
			{
				material.SetTexture("_NearTex", NearTex);
			}
		}

		public void UpdateLightingTex()
		{
			if (material != null)
			{
				material.SetTexture("_LightingTex", LightingTex);
			}
		}

		[ContextMenu("Update Material")]
		public void UpdateMaterial()
		{
			if (material == null)
			{
				material = SgtHelper.CreateTempMaterial("Cloudsphere (Generated)", SgtHelper.ShaderNamePrefix + "Cloudsphere");

				if (model != null)
				{
					model.SetMaterial(material);
				}
			}

			var color = SgtHelper.Brighten(Color, Brightness);

			material.renderQueue = RenderQueue;

			material.SetColor("_Color", color);
			material.SetTexture("_MainTex", MainTex);
			material.SetTexture("_DepthTex", DepthTex);
			material.SetTexture("_LightingTex", LightingTex);

			if (Near == true)
			{
				SgtHelper.EnableKeyword("SGT_A", material); // Near

				material.SetTexture("_NearTex", NearTex);
				material.SetFloat("_NearScale", SgtHelper.Reciprocal(NearDistance));
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_A", material); // Near
			}
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtCloudsphereModel.Create(this);
			}

			var scale = SgtHelper.Divide(Radius, MeshRadius);

			model.SetMesh(Mesh);
			model.SetMaterial(material);
			model.SetScale(scale);
		}

		public static SgtCloudsphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtCloudsphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject  = SgtHelper.CreateGameObject("Cloudsphere", layer, parent, localPosition, localRotation, localScale);
			var cloudsphere = gameObject.AddComponent<SgtCloudsphere>();

			return cloudsphere;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Cloudsphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent      = SgtHelper.GetSelectedParent();
			var cloudsphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(cloudsphere);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull    += CameraPreCull;
			Camera.onPreRender  += CameraPreRender;
			Camera.onPostRender += CameraPostRender;

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}

			UpdateMaterial();
			UpdateModel();
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull    -= CameraPreCull;
			Camera.onPreRender  -= CameraPreRender;
			Camera.onPostRender -= CameraPostRender;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtCloudsphereModel.MarkForDestruction(model);
			SgtHelper.Destroy(material);
		}

		private void CameraPreCull(Camera camera)
		{
			// Write these once to save CPU
			if (renderedThisFrame == false && material != null)
			{
				renderedThisFrame = true;

				// Write lights and shadows
				SgtHelper.SetTempMaterial(material);

				SgtLight.Write(Lit, transform.position, null, null, 1.0f, 1);
				SgtShadow.Write(Lit, gameObject, 1);
			}

			if (CameraOffset != 0.0f)
			{
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

		private void CameraPreRender(Camera camera)
		{
			if (model != null)
			{
				model.Restore(camera);
			}
		}

		private void CameraPostRender(Camera camera)
		{
			renderedThisFrame = false;
		}
	}
}