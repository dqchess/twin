using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtAtmosphere))]
	public class SgtAtmosphere_Editor : SgtEditor<SgtAtmosphere>
	{
		protected override void OnInspector()
		{
			var updateMaterials = false;
			var updateModel     = false;

			DrawDefault("Color", ref updateMaterials, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterials, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterials, "This allows you to adjust the render queue of the atmosphere materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.");

			Separator();

			BeginError(Any(t => t.InnerDepthTex == null));
				DrawDefault("InnerDepthTex", ref updateMaterials, "The look up table associating optical depth with atmospheric color for the planet surface. The left side is used when the atmosphere is thin (e.g. center of the planet when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.InnerMeshRadius <= 0.0f));
				DrawDefault("InnerMeshRadius", ref updateMaterials, ref updateModel, "The radius of the meshes set in the SgtSharedMaterial.");
			EndError();

			Separator();

			BeginError(Any(t => t.OuterDepthTex == null));
				DrawDefault("OuterDepthTex", ref updateMaterials, "The look up table associating optical depth with atmospheric color for the planet sky. The left side is used when the atmosphere is thin (e.g. edge of the atmosphere when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).");
			EndError();
			BeginError(Any(t => t.OuterMeshRadius <= 0.0f));
				DrawDefault("OuterMeshRadius", ref updateModel, "This allows you to set the radius of the OuterMesh. If this is incorrectly set then the atmosphere will render incorrectly.");
			EndError();
			BeginError(Any(t => t.OuterMesh == null));
				DrawDefault("OuterMesh", ref updateModel, "This allows you to set the mesh used to render the atmosphere. This should be a sphere.");
			EndError();

			Separator();

			BeginError(Any(t => t.Height <= 0.0f));
				DrawDefault("Height", ref updateMaterials, ref updateModel, "This allows you to set how high the atmosphere extends above the surface of the planet in local space.");
			EndError();
			BeginError(Any(t => t.InnerFog >= 1.0f));
				DrawDefault("InnerFog", ref updateMaterials, "This allows you to adjust the fog level of the atmosphere on the surface.");
			EndError();
			BeginError(Any(t => t.OuterFog >= 1.0f));
				DrawDefault("OuterFog", ref updateMaterials, "This allows you to adjust the fog level of the atmosphere in the sky.");
			EndError();
			BeginError(Any(t => t.Sky < 0.0f));
				DrawDefault("Sky", "This allows you to control how thick the atmosphere is when the camera is inside its radius"); // Updated when rendering
			EndError();
			DrawDefault("CameraOffset", "This allows you to offset the camera distance in world space when rendering the atmosphere, giving you fine control over the render order."); // Updated automatically

			Separator();

			DrawDefault("Lit", ref updateMaterials, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(t => t.Lit == true))
			{
				BeginIndent();
					BeginError(Any(t => t.LightingTex == null));
						DrawDefault("LightingTex", ref updateMaterials, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
					DrawDefault("Scattering", ref updateMaterials, "If you enable this then light will scatter through the atmosphere. This means light entering the eye will come from all angles, especially around the light point.");
					if (Any(t => t.Scattering == true))
					{
						BeginIndent();
							DrawDefault("GroundScattering", ref updateMaterials, "If you enable this then atmospheric scattering will be applied to the surface material.");
							BeginError(Any(t => t.ScatteringTex == null));
								DrawDefault("ScatteringTex", ref updateMaterials, "The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
							EndError();
							DrawDefault("ScatteringStrength", ref updateMaterials, "The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.");
							DrawDefault("ScatteringMie", ref updateMaterials, "The mie scattering term, allowing you to adjust the distribution of front scattered light.");
							DrawDefault("ScatteringRayleigh", ref updateMaterials, "The mie rayleigh term, allowing you to adjust the distribution of front and back scattered light.");
						EndIndent();
					}
					DrawDefault("Night"); // Updated automatically
					if (Any(t => t.Night == true))
					{
						BeginIndent();
						DrawDefault("NightSky"); // Updated automatically
						DrawDefault("NightEase"); // Updated automatically
						BeginError(Any(t => t.NightStart >= t.NightEnd));
							DrawDefault("NightStart"); // Updated automatically
							DrawDefault("NightEnd"); // Updated automatically
						EndError();
						BeginError(Any(t => t.NightPower < 1.0f));
							DrawDefault("NightPower"); // Updated automatically
						EndError();
						EndIndent();
					}
				EndIndent();
			}

			if (Any(t => (t.InnerDepthTex == null || t.OuterDepthTex == null) && t.GetComponent<SgtAtmosphereDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAtmosphereDepthTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtAtmosphereLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAtmosphereLightingTex>(t.gameObject));
				}
			}

			if (Any(t => t.Lit == true && t.Scattering == true && t.ScatteringTex == null && t.GetComponent<SgtAtmosphereScatteringTex>() == null))
			{
				Separator();

				if (Button("Add ScatteringTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtAtmosphereScatteringTex>(t.gameObject));
				}
			}

			if (Any(t => SetOuterMeshAndOuterMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Outer Mesh & Outer Mesh Radius") == true)
				{
					Each(t => SetOuterMeshAndOuterMeshRadius(t, true));
				}
			}

			if (Any(t => AddInnerRendererAndSetInnerMeshRadius(t, false)))
			{
				Separator();

				if (Button("Add Inner Renderer & Set Inner Mesh Radius") == true)
				{
					Each(t => AddInnerRendererAndSetInnerMeshRadius(t, true));
				}
			}

			if (updateMaterials == true) DirtyEach(t => t.UpdateMaterials());
			if (updateModel     == true) DirtyEach(t => t.UpdateModel    ());
		}

		private bool SetOuterMeshAndOuterMeshRadius(SgtAtmosphere atmosphere, bool apply)
		{
			if (atmosphere.OuterMesh == null)
			{
				var mesh = SgtHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						atmosphere.OuterMesh       = mesh;
						atmosphere.OuterMeshRadius = SgtHelper.GetMeshRadius(mesh);

						atmosphere.UpdateMaterials();
						atmosphere.UpdateModel();
					}

					return true;
				}
			}

			return false;
		}

		private bool AddInnerRendererAndSetInnerMeshRadius(SgtAtmosphere atmosphere, bool apply)
		{
			if (atmosphere.CachedSharedMaterial.RendererCount == 0)
			{
				var meshRenderer = atmosphere.GetComponentInParent<MeshRenderer>();

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
								atmosphere.CachedSharedMaterial.AddRenderer(meshRenderer);
								atmosphere.InnerMeshRadius = SgtHelper.GetMeshRadius(mesh);
								atmosphere.UpdateModel();
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
	/// <summary>This component allows you to draw a volumetric atmosphere. The atmosphere is rendered using two materials, one for the surface (inner), and one for the sky (outer).
	/// The outer part of the atmosphere is automatically generated by this component using the OuterMesh you specify.
	/// The inner part of the atmosphere is provided by you (e.g. a normal sphere GameObject), and is specified in the SgtSharedMaterial component that this component automatically adds.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtSharedMaterial))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtAtmosphere")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Atmosphere")]
	public class SgtAtmosphere : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color = Color.white;

		/// <summary>The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness = 1.0f;

		/// <summary>This allows you to adjust the render queue of the atmosphere materials. You can normally adjust the render queue in the material settings, but since these materials are procedurally generated your changes will be lost.</summary>
		public SgtRenderQueue RenderQueue = SgtRenderQueue.GroupType.Transparent;

		/// <summary>The look up table associating optical depth with atmospheric color for the planet surface. The left side is used when the atmosphere is thin (e.g. center of the planet when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).</summary>
		public Texture InnerDepthTex;

		/// <summary>The radius of the meshes set in the SgtSharedMaterial.</summary>
		public float InnerMeshRadius = 1.0f;

		/// <summary>The look up table associating optical depth with atmospheric color for the planet sky. The left side is used when the atmosphere is thin (e.g. edge of the atmosphere when looking from space). The right side is used when the atmosphere is thick (e.g. the horizon).</summary>
		public Texture2D OuterDepthTex;

		/// <summary>This allows you to set the mesh used to render the atmosphere. This should be a sphere.</summary>
		public Mesh OuterMesh;

		/// <summary>This allows you to set the radius of the OuterMesh. If this is incorrectly set then the atmosphere will render incorrectly.</summary>
		public float OuterMeshRadius = 1.0f;

		/// <summary>This allows you to set how high the atmosphere extends above the surface of the planet in local space.</summary>
		public float Height = 0.1f;

		/// <summary>This allows you to adjust the fog level of the atmosphere on the surface.</summary>
		public float InnerFog;

		/// <summary>This allows you to adjust the fog level of the atmosphere in the sky.</summary>
		public float OuterFog;

		/// <summary>This allows you to control how thick the atmosphere is when the camera is inside its radius.</summary>
		public float Sky = 1.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the atmosphere, giving you fine control over the render order.</summary>
		public float CameraOffset;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		/// <summary>If you enable this then light will scatter through the atmosphere. This means light entering the eye will come from all angles, especially around the light point.</summary>
		public bool Scattering;

		/// <summary>If you enable this then atmospheric scattering will be applied to the surface material.</summary>
		public bool GroundScattering;

		/// <summary>The look up table associating light angle with scattering color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture ScatteringTex;

		/// <summary>The scattering is multiplied by this value, allowing you to easily adjust the brightness of the effect.</summary>
		public float ScatteringStrength = 3.0f;

		/// <summary>The mie scattering term, allowing you to adjust the distribution of front scattered light.</summary>
		public float ScatteringMie = 50.0f;

		/// <summary>The mie rayleigh term, allowing you to adjust the distribution of front and back scattered light.</summary>
		public float ScatteringRayleigh = 0.1f;

		[Tooltip("Should the night side of the atmosphere have different sky values?")]
		public bool Night;

		[Tooltip("The 'Sky' value of the night side")]
		public float NightSky = 0.25f;

		[Tooltip("The transition style between the day and night")]
		public SgtEase.Type NightEase = SgtEase.Type.Smoothstep;

		[Tooltip("The start point of the day/sunset transition (0 = dark side, 1 = light side)")]
		[Range(0.0f, 1.0f)]
		public float NightStart = 0.4f;

		[Tooltip("The end point of the day/sunset transition (0 = dark side, 1 = light side)")]
		[Range(0.0f, 1.0f)]
		public float NightEnd = 0.6f;

		[Tooltip("The power of the night transition")]
		public float NightPower = 2.0f;

		// The GameObjects used to render the sky
		[SerializeField]
		private SgtAtmosphereModel model;

		// The material applied to the surface
		[System.NonSerialized]
		private Material innerMaterial;

		// The material applied to the sky
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

		public Material InnerMaterial
		{
			get
			{
				return innerMaterial;
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

		public void UpdateLightingTex()
		{
			if (innerMaterial != null)
			{
				innerMaterial.SetTexture("_LightingTex", LightingTex);
			}

			if (outerMaterial != null)
			{
				outerMaterial.SetTexture("_LightingTex", LightingTex);
			}
		}

		public void UpdateScatteringTex()
		{
			if (innerMaterial != null)
			{
				innerMaterial.SetTexture("_ScatteringTex", ScatteringTex);
			}

			if (outerMaterial != null)
			{
				outerMaterial.SetTexture("_ScatteringTex", ScatteringTex);
			}
		}

		[ContextMenu("Update Materials")]
		public void UpdateMaterials()
		{
			if (innerMaterial == null)
			{
				innerMaterial = SgtHelper.CreateTempMaterial("Atmosphere Inner (Generated)", SgtHelper.ShaderNamePrefix + "AtmosphereInner");

				CachedSharedMaterial.Material = innerMaterial;
			}

			if (outerMaterial == null)
			{
				outerMaterial = SgtHelper.CreateTempMaterial("Atmosphere Outer (Generated)", SgtHelper.ShaderNamePrefix + "AtmosphereOuter");

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

			if (Lit == true)
			{
				innerMaterial.SetTexture("_LightingTex", LightingTex);
				outerMaterial.SetTexture("_LightingTex", LightingTex);

				if (Scattering == true)
				{
					outerMaterial.SetTexture("_ScatteringTex", ScatteringTex);
					outerMaterial.SetFloat("_ScatteringMie", ScatteringMie);
					outerMaterial.SetFloat("_ScatteringRayleigh", ScatteringRayleigh);

					SgtHelper.EnableKeyword("SGT_B", outerMaterial); // Scattering

					if (GroundScattering == true)
					{
						innerMaterial.SetTexture("_ScatteringTex", ScatteringTex);
						innerMaterial.SetFloat("_ScatteringMie", ScatteringMie);
						innerMaterial.SetFloat("_ScatteringRayleigh", ScatteringRayleigh);

						SgtHelper.EnableKeyword("SGT_B", innerMaterial); // Scattering
					}
					else
					{
						SgtHelper.DisableKeyword("SGT_B", innerMaterial); // Scattering
					}
				}
				else
				{
					SgtHelper.DisableKeyword("SGT_B", innerMaterial); // Scattering
					SgtHelper.DisableKeyword("SGT_B", outerMaterial); // Scattering
				}
			}

			UpdateMaterialNonSerialized();
		}

		[ContextMenu("Update Model")]
		public void UpdateModel()
		{
			if (model == null)
			{
				model = SgtAtmosphereModel.Create(this);
			}

			var scale = SgtHelper.Divide(OuterRadius, OuterMeshRadius);

			model.SetMesh(OuterMesh);
			model.SetMaterial(outerMaterial);
			model.SetScale(scale);
		}

		public static SgtAtmosphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtAtmosphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Atmosphere", layer, parent, localPosition, localRotation, localScale);
			var atmosphere = gameObject.AddComponent<SgtAtmosphere>();

			return atmosphere;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Atmosphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent     = SgtHelper.GetSelectedParent();
			var atmosphere = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(atmosphere);
		}
#endif

		protected virtual void OnEnable()
		{
			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;
			SgtFloatingCamera.OnPositionChanged += FloatingCameraPositionChanged;

			CachedSharedMaterial.Material = innerMaterial;

			if (model != null)
			{
				model.gameObject.SetActive(true);
			}

			UpdateMaterials();
			UpdateModel();
		}

		protected virtual void LateUpdate()
		{
			// The lights and shadows may have moved, so write them
			if (innerMaterial != null && outerMaterial != null)
			{
				SgtHelper.SetTempMaterial(innerMaterial, outerMaterial);

				SgtLight.Write(Lit, transform.position, transform, null, ScatteringStrength, 2);
				SgtShadow.Write(Lit, gameObject, 2);
			}
		}

		protected virtual void OnDisable()
		{
			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;
			SgtFloatingCamera.OnPositionChanged -= FloatingCameraPositionChanged;

			cachedSharedMaterial.Material = null;

			if (model != null)
			{
				model.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			SgtAtmosphereModel.MarkForDestruction(model);
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
					outerMaterial.SetFloat("_Sky", GetSky(camera) * OuterDepthTex.GetPixelBilinear(clampedSky / outerDensity, 0.0f).a);
				}

				UpdateMaterialNonSerialized();
			}
		}

		private void FloatingCameraPositionChanged(SgtFloatingCamera floatingCamera)
		{
			LateUpdate();
		}

		private float GetSky(Camera camera)
		{
			/*
			if (Lit == true && Night == true && Lights != null)
			{
				var lighting        = 0.0f;
				var lightCount      = 0;
				var cameraDirection = (camera.transform.position - transform.position).normalized;

				for (var i = 0; i < Lights.Count && lightCount < 2; i++)
				{
					var light = Lights[i];

					if (SgtHelper.Enabled(light) == true && light.intensity > 0.0f)
					{
						var position  = default(Vector3);
						var direction = default(Vector3);
						var color     = default(Color);

						lightCount += 1;

						SgtHelper.CalculateLight(light, transform.position, transform, null, ref position, ref direction, ref color);

						var dot     = Vector3.Dot(direction, cameraDirection) * 0.5f + 0.5f;
						var night01 = Mathf.InverseLerp(NightEnd, NightStart, dot);
						var night   = SgtEase.Evaluate(NightEase, 1.0f - Mathf.Pow(night01, NightPower));

						if (night > lighting)
						{
							lighting = night;
						}
					}
				}

				return Mathf.Lerp(NightSky, Sky, lighting);
			}
			*/
		
			return Sky;
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