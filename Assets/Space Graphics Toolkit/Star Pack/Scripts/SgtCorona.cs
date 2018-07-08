using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtCorona))]
	public class SgtCorona_Editor : SgtEditor<SgtCorona>
	{
		protected override void OnInspector()
		{
			var updateMaterials = false;
			var updateModel     = false;

			DrawDefault("Color", ref updateMaterials, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterials, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterials, "This allows you to adjust the render queue of the corona materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.InnerDepthTex == null));
				DrawDefault("InnerDepthTex", ref updateMaterials, "The look up table associating optical depth with coronal color for the star surface. The left side is used when the corona is thin (e.g. center of the star when looking from space). The right side is used when the corona is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.InnerMeshRadius <= 0.0f));
				DrawDefault("InnerMeshRadius", ref updateMaterials, ref updateModel, "The radius of the inner renderers (surface) in local coordinates.");
			EndError();

			Separator();

			BeginError(Any(t => t.OuterDepthTex == null));
				DrawDefault("OuterDepthTex", ref updateMaterials, "The look up table associating optical depth with coronal color for the star sky. The left side is used when the corona is thin (e.g. edge of the corona when looking from space). The right side is used when the corona is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.OuterMesh == null));
				DrawDefault("OuterMesh", ref updateModel, "This allows you to set the mesh used to render the atmosphere. This should be a sphere.");
			EndError();
			BeginError(Any(t => t.OuterMeshRadius <= 0.0f));
				DrawDefault("OuterMeshRadius", ref updateModel, "This allows you to set the radius of the OuterMesh. If this is incorrectly set then the corona will render incorrectly.");
			EndError();

			Separator();

			BeginError(Any(t => t.Height <= 0.0f));
				DrawDefault("Height", ref updateMaterials, ref updateModel, "This allows you to set how high the corona extends above the surface of the star in local space.");
			EndError();
			BeginError(Any(t => t.InnerFog >= 1.0f));
				DrawDefault("InnerFog", ref updateMaterials, "If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).");
			EndError();
			BeginError(Any(t => t.OuterFog >= 1.0f));
				DrawDefault("OuterFog", ref updateMaterials, "If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).");
			EndError();
			BeginError(Any(t => t.Sky < 0.0f));
				DrawDefault("Sky", "This allows you to control how thick the corona is when the camera is inside its radius."); // Updated when rendering
			EndError();
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the corona, giving you fine control over the render order."); // Updated automatically

			if (Any(t => (t.InnerDepthTex == null || t.OuterDepthTex == null) && t.GetComponent<SgtCoronaDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtCoronaDepthTex>(t.gameObject));
				}
			}

			if (Any(t => SetOuterMeshAndOuterMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set OuterMesh & OuterMeshRadius") == true)
				{
					Each(t => SetOuterMeshAndOuterMeshRadius(t, true));
				}
			}

			if (Any(t => AddInnerRendererAndSetInnerMeshRadius(t, false)))
			{
				Separator();

				if (Button("Add InnerRenderer & Set InnerMeshRadius") == true)
				{
					Each(t => AddInnerRendererAndSetInnerMeshRadius(t, true));
				}
			}

			if (updateMaterials == true) DirtyEach(t => t.UpdateMaterials());
			if (updateModel     == true) DirtyEach(t => t.UpdateModel    ());
		}

		private bool SetOuterMeshAndOuterMeshRadius(SgtCorona corona, bool apply)
		{
			if (corona.OuterMesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						corona.OuterMesh       = mesh;
						corona.OuterMeshRadius = SgtHelper.GetMeshRadius(mesh);

						corona.UpdateMaterials();
						corona.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}

		private bool AddInnerRendererAndSetInnerMeshRadius(SgtCorona corona, bool apply)
		{
			if (corona.CachedSharedMaterial.RendererCount == 0)
			{
				var meshRenderer = corona.GetComponentInParent<MeshRenderer>();

				if (meshRenderer != null)
				{
					var meshFilter = meshRenderer.GetComponent<MeshFilter>();

					if (meshFilter != null)
					{
						var mesh = meshFilter.sharedMesh;

						if (mesh != null)
						{
							if (apply == true)
							{
								corona.CachedSharedMaterial.AddRenderer(meshRenderer);
								corona.InnerMeshRadius = SgtHelper.GetMeshRadius(mesh);
								corona.UpdateModel();
							}

							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to draw a volumetric corona around a sphere.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtSharedMaterial))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtCorona")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Corona")]
	public class SgtCorona : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the corona materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>This allows you to set how high the corona extends above the surface of the star in local space.</summary>
		public float Height = 0.1f;

		/// <summary>If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).</summary>
		public float InnerFog;

		/// <summary>If you want an extra-thin or extra-thick density, you can adjust that here (0 = default).</summary>
		public float OuterFog;

		/// <summary>This allows you to control how thick the corona is when the camera is inside its radius.</summary>
		public float Sky = 1.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the corona, giving you fine control over the render order.</summary>
		public float CameraOffset;

		/// <summary>The look up table associating optical depth with coronal color for the star surface. The left side is used when the corona is thin (e.g. center of the star when looking from space). The right side is used when the corona is thick (e.g. the horizon).</summary>
		public Texture InnerDepthTex;

		/// <summary>The radius of the inner renderers (surface) in local coordinates.</summary>
		public float InnerMeshRadius = 1.0f;

		/// <summary>The look up table associating optical depth with coronal color for the star sky. The left side is used when the corona is thin (e.g. edge of the corona when looking from space). The right side is used when the corona is thick (e.g. the horizon).</summary>
		public Texture2D OuterDepthTex;

		/// <summary>This allows you to set the mesh used to render the atmosphere. This should be a sphere.</summary>
		public Mesh OuterMesh;

		/// <summary>This allows you to set the radius of the OuterMesh. If this is incorrectly set then the corona will render incorrectly.</summary>
		public float OuterMeshRadius = 1.0f;

		/// <summary>Each model is used to render one segment of the disc.</summary>
		[SerializeField]
		private SgtCoronaModel model;

		/// <summary>The material applied to all inner renderers.</summary>
		[System.NonSerialized]
		private Material innerMaterial;

		/// <summary>The material applied to the outer model.</summary>
		[System.NonSerialized]
		private Material outerMaterial;

		[System.NonSerialized]
		private SgtSharedMaterial cachedSharedMaterial;

		[System.NonSerialized]
		private bool cachedSharedMaterialSet;

		public float OuterRadius
		{
			get
			{
				return InnerMeshRadius + Height;
			}
		}

		public SgtSharedMaterial CachedSharedMaterial
		{
			get
			{
				if (cachedSharedMaterialSet == false)
				{
					cachedSharedMaterial    = GetComponent<SgtSharedMaterial>();
					cachedSharedMaterialSet = true;
				}

				return cachedSharedMaterial;
			}
		}

		public void UpdateInnerDepthTex()
		{
			if (innerMaterial != null)
			{
				innerMaterial.SetTexture("_DepthTex", InnerDepthTex);
			}
		}

		public void UpdateOuterDepthTex()
		{
			if (outerMaterial != null)
			{
				outerMaterial.SetTexture("_DepthTex", OuterDepthTex);
			}
		}

		[ContextMenu("Update Materials")]
		public void UpdateMaterials()
		{
			if (innerMaterial == null)
			{
				innerMaterial = SgtHelper.CreateTempMaterial("Corona Inner (Generated)", SgtHelper.ShaderNamePrefix + "CoronaInner");

				CachedSharedMaterial.Material = innerMaterial;
			}

			if (outerMaterial == null)
			{
				outerMaterial = SgtHelper.CreateTempMaterial("Corona Outer (Generated)", SgtHelper.ShaderNamePrefix + "CoronaOuter");

				if (model != null)
				{
					model.SetMaterial(outerMaterial);
				}
			}

			var color = SgtHelper.Brighten(Color, Brightness);

			innerMaterial.renderQueue = outerMaterial.renderQueue = RenderQueue;

			innerMaterial.SetColor("_Color", color);
			outerMaterial.SetColor("_Color", color);

			innerMaterial.SetTexture("_DepthTex", InnerDepthTex);
			outerMaterial.SetTexture("_DepthTex", OuterDepthTex);

			UpdateMaterialNonSerialized();
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtCoronaModel.Create(this);
			}

			var scale = SgtHelper.Divide(OuterRadius, OuterMeshRadius);

			model.SetMesh(OuterMesh);
			model.SetMaterial(outerMaterial);
			model.SetScale(scale);
		}

		public static SgtCorona Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtCorona Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Corona", layer, parent, localPosition, localRotation, localScale);
			var corona     = gameObject.AddComponent<SgtCorona>();

			return corona;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Corona", false, 10)]
		public static void CreateMenuItem()
		{
			var parent = SgtHelper.GetSelectedParent();
			var corona = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(corona);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;

			CachedSharedMaterial.Material = innerMaterial;

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}

			UpdateMaterials();
			UpdateModel();
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;

			cachedSharedMaterial.Material = null;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtCoronaModel.MarkForDestruction(model);
			SgtHelper.Destroy(outerMaterial);
			SgtHelper.Destroy(innerMaterial);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (SgtHelper.Enabled(this) == true)
			{
				var r1 = InnerMeshRadius;
				var r2 = OuterRadius;

				SgtHelper.DrawSphere(transform.position, transform.right * transform.lossyScale.x * r1, transform.up * transform.lossyScale.y * r1, transform.forward * transform.lossyScale.z * r1);
				SgtHelper.DrawSphere(transform.position, transform.right * transform.lossyScale.x * r2, transform.up * transform.lossyScale.y * r2, transform.forward * transform.lossyScale.z * r2);
			}
		}
#endif

		private void CameraPreCull(Camera camera)
		{
			if (model != null)
			{
				model.Revert();
				{
					if (CameraOffset != 0.0f)
					{
						var direction = camera.transform.position - transform.position;

						model.transform.position += direction.normalized * CameraOffset;
					}
				}
				model.Save(camera);
			}
		}

		private void CameraPreRender(Camera camera)
		{
			if (model != null)
			{
				model.Restore(camera);
			}

			// Write camera-dependant shader values
			if (innerMaterial != null && outerMaterial != null)
			{
				var cameraPosition       = camera.transform.position;
				var localCameraPosition  = transform.InverseTransformPoint(cameraPosition);
				var localDistance        = localCameraPosition.magnitude;
				var clampedSky           = Mathf.InverseLerp(OuterRadius, InnerMeshRadius, localDistance);
				var innerAtmosphereDepth = default(float);
				var outerAtmosphereDepth = default(float);
				var radiusRatio          = SgtHelper.Divide(InnerMeshRadius, OuterRadius);
				var scaleDistance        = SgtHelper.Divide(localDistance, OuterRadius);
				var innerDensity         = 1.0f - InnerFog;
				var outerDensity         = 1.0f - OuterFog;

				SgtHelper.CalculateAtmosphereThicknessAtHorizon(radiusRatio, 1.0f, scaleDistance, out innerAtmosphereDepth, out outerAtmosphereDepth);

				SgtHelper.SetTempMaterial(innerMaterial, outerMaterial);

				if (scaleDistance > 1.0f)
				{
					SgtHelper.EnableKeyword("SGT_A"); // Outside
				}
				else
				{
					SgtHelper.DisableKeyword("SGT_A"); // Outside
				}

				innerMaterial.SetFloat("_HorizonLengthRecip", SgtHelper.Reciprocal(innerAtmosphereDepth * innerDensity));
				outerMaterial.SetFloat("_HorizonLengthRecip", SgtHelper.Reciprocal(outerAtmosphereDepth * outerDensity));

				if (OuterDepthTex != null)
				{
#if UNITY_EDITOR
					SgtHelper.MakeTextureReadable(OuterDepthTex);
#endif
					outerMaterial.SetFloat("_Sky", Sky * OuterDepthTex.GetPixelBilinear(clampedSky / outerDensity, 0.0f).a);
				}

				UpdateMaterialNonSerialized();
			}
		}

		private void UpdateMaterialNonSerialized()
		{
			var scale        = SgtHelper.Divide(OuterMeshRadius, OuterRadius);
			var worldToLocal = SgtHelper.Scaling(scale) * transform.worldToLocalMatrix;

			innerMaterial.SetMatrix("_WorldToLocal", worldToLocal);
			outerMaterial.SetMatrix("_WorldToLocal", worldToLocal);
		}
	}
}