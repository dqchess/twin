using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	public abstract class SgtBelt_Editor<T> : SgtQuads_Editor<T>
		where T : SgtBelt
	{
		protected override void DrawMaterial(ref bool updateMaterial)
		{
			DrawDefault("Color", ref updateMaterial, "The base color will be multiplied by this.");
			BeginError(Any(t => t.Brightness < 0.0f));
				DrawDefault("Brightness", ref updateMaterial, "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			EndError();
			DrawDefault("RenderQueue", ref updateMaterial, "This allows you to adjust the render queue of the belt material. You can normally adjust the render queue in the material settings, but since this material is procedurally generated your changes will be lost.");
			DrawDefault("OrbitOffset", "The amount of seconds this belt has been animating for."); // Updated automatically
			DrawDefault("OrbitSpeed", "The animation speed of this belt."); // Updated automatically
		}

		protected void DrawLighting(ref bool updateMaterial)
		{
			DrawDefault("Lit", ref updateMaterial, "If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.");

			if (Any(t => t.Lit == true))
			{
				BeginIndent();
					BeginError(Any(t => t.LightingTex == null));
						DrawDefault("LightingTex", ref updateMaterial, "The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.");
					EndError();
				EndIndent();
			}

			if (Any(t => t.Lit == true && t.LightingTex == null && t.GetComponent<SgtBeltLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(t => SgtHelper.GetOrAddComponent<SgtBeltLightingTex>(t.gameObject));
				}
			}
		}

		protected override void DrawMainTex(ref bool updateMaterial, ref bool updateMeshesAndModels)
		{
			BeginError(Any(t => t.MainTex == null));
				DrawDefault("MainTex", ref updateMaterial, "The main texture of this material.");
			EndError();
			BeginError(Any(t => t.HeightTex == null));
				DrawDefault("HeightTex", ref updateMaterial, "The height texture of this belt.");
			EndError();
			DrawLayout(ref updateMaterial, ref updateMeshesAndModels);
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This base class contains the functionality to render an asteroid belt.</summary>
	public abstract class SgtBelt : SgtQuads
	{
		/// <summary>The height texture of this belt.</summary>
		public Texture HeightTex;

		/// <summary>The amount of seconds this belt has been animating for.</summary>
		public float OrbitOffset;

		/// <summary>The animation speed of this belt.</summary>
		public float OrbitSpeed = 1.0f;

		/// <summary>If you enable this then nearby SgtLight and SgtShadow casters will be found and applied to the lighting calculations.</summary>
		public bool Lit;

		/// <summary>The look up table associating light angle with surface color. The left side is used on the dark side, the middle is used on the horizon, and the right side is used on the light side.</summary>
		public Texture LightingTex;

		/// <summary>This is used to optimize shader calculations.</summary>
		[System.NonSerialized]
		private bool renderedThisFrame;

		protected override string ShaderName
		{
			get
			{
				return SgtHelper.ShaderNamePrefix + "Belt";
			}
		}

		public SgtBeltCustom MakeEditableCopy(int layer = 0, Transform parent = null)
		{
			return MakeEditableCopy(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public SgtBeltCustom MakeEditableCopy(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
#if UNITY_EDITOR
			SgtHelper.BeginUndo("Create Editable Belt Copy");
#endif
			var gameObject = SgtHelper.CreateGameObject("Editable Belt Copy", layer, parent, localPosition, localRotation, localScale);
			var customBelt = SgtHelper.AddComponent<SgtBeltCustom>(gameObject, false);
			var quads      = new List<SgtBeltAsteroid>();
			var quadCount  = BeginQuads();

			for (var i = 0; i < quadCount; i++)
			{
				var asteroid = SgtPoolClass<SgtBeltAsteroid>.Pop() ?? new SgtBeltAsteroid();

				NextQuad(ref asteroid, i);

				quads.Add(asteroid);
			}

			EndQuads();

			// Copy common settings
			customBelt.Color         = Color;
			customBelt.Brightness    = Brightness;
			customBelt.MainTex       = MainTex;
			customBelt.HeightTex     = HeightTex;
			customBelt.Layout        = Layout;
			customBelt.LayoutColumns = LayoutColumns;
			customBelt.LayoutRows    = LayoutRows;
			customBelt.LayoutRects   = new List<Rect>(LayoutRects);
			customBelt.BlendMode     = BlendMode;
			customBelt.RenderQueue   = RenderQueue;
			customBelt.OrbitOffset   = OrbitOffset;
			customBelt.OrbitSpeed    = OrbitSpeed;
			customBelt.Lit           = Lit;
			customBelt.LightingTex   = LightingTex;

			// Copy custom settings
			customBelt.Asteroids = quads;

			// Update
			customBelt.UpdateMaterial();
			customBelt.UpdateMeshesAndModels();

			return customBelt;
		}

		public virtual void UpdateLightingTex()
		{
			if (material != null)
			{
				material.SetTexture("_LightingTex", LightingTex);
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Make Editable Copy")]
		public void MakeEditableCopyContext()
		{
			var customBelt = MakeEditableCopy(gameObject.layer, transform.parent, transform.localPosition, transform.localRotation, transform.localScale);

			SgtHelper.SelectAndPing(customBelt);
		}
#endif

		protected override void OnEnable()
		{
			Camera.onPreRender  += CameraPreRender;
			Camera.onPostRender += CameraPostRender;

			base.OnEnable();
		}

		protected virtual void LateUpdate()
		{
			if (Application.isPlaying == true)
			{
				OrbitOffset += Time.deltaTime * OrbitSpeed;
			}

			if (material != null)
			{
				material.SetFloat("_Age", OrbitOffset);
			}
		}

		protected override void OnDisable()
		{
			Camera.onPreRender  -= CameraPreRender;
			Camera.onPostRender -= CameraPostRender;

			base.OnDisable();
		}

		protected override void BuildMaterial()
		{
			base.BuildMaterial();

			if (BlendMode == SgtQuads.BlendModeType.Default)
			{
				BuildAlphaTest();
			}

			material.SetTexture("_HeightTex", HeightTex);
			material.SetFloat("_Age", OrbitOffset);

			if (Lit == true)
			{
				material.SetTexture("_LightingTex", LightingTex);
			}
		}

		protected abstract void NextQuad(ref SgtBeltAsteroid quad, int starIndex);

		protected override void BuildMesh(Mesh mesh, int asteroidIndex, int asteroidCount)
		{
			var positions = new Vector3[asteroidCount * 4];
			var colors    = new Color[asteroidCount * 4];
			var normals   = new Vector3[asteroidCount * 4];
			var tangents  = new Vector4[asteroidCount * 4];
			var coords1   = new Vector2[asteroidCount * 4];
			var coords2   = new Vector2[asteroidCount * 4];
			var indices   = new int[asteroidCount * 6];
			var maxWidth  = 0.0f;
			var maxHeight = 0.0f;

			for (var i = 0; i < asteroidCount; i++)
			{
				NextQuad(ref SgtBeltAsteroid.Temp, asteroidIndex + i);

				var offV     = i * 4;
				var offI     = i * 6;
				var radius   = SgtBeltAsteroid.Temp.Radius;
				var distance = SgtBeltAsteroid.Temp.OrbitDistance;
				var height   = SgtBeltAsteroid.Temp.Height;
				var uv       = tempCoords[SgtHelper.Mod(SgtBeltAsteroid.Temp.Variant, tempCoords.Count)];

				maxWidth  = Mathf.Max(maxWidth , distance + radius);
				maxHeight = Mathf.Max(maxHeight, height   + radius);

				positions[offV + 0] =
				positions[offV + 1] =
				positions[offV + 2] =
				positions[offV + 3] = new Vector3(SgtBeltAsteroid.Temp.OrbitAngle, distance, SgtBeltAsteroid.Temp.OrbitSpeed);

				colors[offV + 0] =
				colors[offV + 1] =
				colors[offV + 2] =
				colors[offV + 3] = SgtBeltAsteroid.Temp.Color;

				normals[offV + 0] = new Vector3(-1.0f,  1.0f, 0.0f);
				normals[offV + 1] = new Vector3( 1.0f,  1.0f, 0.0f);
				normals[offV + 2] = new Vector3(-1.0f, -1.0f, 0.0f);
				normals[offV + 3] = new Vector3( 1.0f, -1.0f, 0.0f);

				tangents[offV + 0] =
				tangents[offV + 1] =
				tangents[offV + 2] =
				tangents[offV + 3] = new Vector4(SgtBeltAsteroid.Temp.Angle / Mathf.PI, SgtBeltAsteroid.Temp.Spin / Mathf.PI, 0.0f, 0.0f);

				coords1[offV + 0] = new Vector2(uv.x, uv.y);
				coords1[offV + 1] = new Vector2(uv.z, uv.y);
				coords1[offV + 2] = new Vector2(uv.x, uv.w);
				coords1[offV + 3] = new Vector2(uv.z, uv.w);

				coords2[offV + 0] =
				coords2[offV + 1] =
				coords2[offV + 2] =
				coords2[offV + 3] = new Vector2(radius, height);

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.vertices  = positions;
			mesh.colors    = colors;
			mesh.normals   = normals;
			mesh.tangents  = tangents;
			mesh.uv        = coords1;
			mesh.uv2       = coords2;
			mesh.triangles = indices;
			mesh.bounds    = new Bounds(Vector3.zero, new Vector3(maxWidth * 2.0f, maxHeight * 2.0f, maxWidth * 2.0f));
		}

		private void ObserverPreRender(SgtCamera observer)
		{
			if (material != null)
			{
				material.SetFloat("_CameraRollAngle", observer.RollAngle * Mathf.Deg2Rad);
			}
		}

		protected void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				var observer = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref observer) == true)
				{
					material.SetFloat("_CameraRollAngle", observer.RollAngle * Mathf.Deg2Rad);
				}
				else
				{
					material.SetFloat("_CameraRollAngle", 0.0f);
				}

				// Write these once to save CPU
				if (renderedThisFrame == false && material != null)
				{
					renderedThisFrame = true;

					// Write lights and shadows
					SgtHelper.SetTempMaterial(material);

					SgtLight.Write(Lit, transform.position, transform, null, 1.0f, 2);
					SgtShadow.Write(Lit, gameObject, 2);
				}
			}
		}

		private void CameraPostRender(Camera camera)
		{
			renderedThisFrame = false;
		}
	}
}