using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtBackdrop))]
	public class SgtBackdrop_Editor : SgtQuads_Editor<SgtBackdrop>
	{
		protected override void OnInspector()
		{
			var updateMaterial        = false;
			var updateMeshesAndModels = false;

			DrawMaterial(ref updateMaterial);

			Separator();

			DrawMainTex(ref updateMaterial, ref updateMeshesAndModels);
			DrawLayout(ref updateMaterial, ref updateMeshesAndModels);

			Separator();

			DrawDefault("Seed", ref updateMeshesAndModels, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(t => t.Radius <= 0.0f));
				DrawDefault("Radius", ref updateMeshesAndModels, "The radius of the starfield.");
			EndError();
			DrawDefault("Squash", ref updateMeshesAndModels, "Should more stars be placed near the horizon?");

			Separator();

			BeginError(Any(t => t.StarCount < 0));
				DrawDefault("StarCount", ref updateMeshesAndModels, "The amount of stars that will be generated in the starfield.");
			EndError();
			DrawDefault("StarColors", ref updateMeshesAndModels, "Each star is given a random color from this gradient.");
			BeginError(Any(t => t.StarRadiusMin < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMin", ref updateMeshesAndModels, "The minimum radius of stars in the starfield.");
			EndError();
			BeginError(Any(t => t.StarRadiusMax < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				DrawDefault("StarRadiusMax", ref updateMeshesAndModels, "The maximum radius of stars in the starfield.");
			EndError();
			DrawDefault("StarRadiusBias", ref updateMeshesAndModels, "How likely the size picking will pick smaller stars over larger ones (1 = default/linear).");

			Separator();

			DrawDefault("PowerRgb", ref updateMaterial, "Instead of just tinting the stars with the colors, should the RGB values be raised to the power of the color?");

			RequireCamera();

			if (updateMaterial        == true) DirtyEach(t => t.UpdateMaterial       ());
			if (updateMeshesAndModels == true) DirtyEach(t => t.UpdateMeshesAndModels());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to generate procedurally placed quads on the edge of a sphere.
	/// The quads can then be textured using clouds or stars, and will follow the rendering camera, creating a backdrop.
	/// This backdrop is very quick to render, and provides a good alternative to skyboxes because of the vastly reduced memory requirements.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtBackdrop")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Backdrop")]
	public class SgtBackdrop : SgtQuads
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		/// <summary>The radius of the starfield.</summary>
		public float Radius = 1.0f;

		/// <summary>Should more stars be placed near the horizon?</summary>
		[Range(0.0f, 1.0f)]
		public float Squash = 0.0f;

		/// <summary>Instead of just tinting the stars with the colors, should the RGB values be raised to the power of the color?</summary>
		public bool PowerRgb;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin = 0.01f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias = 0.0f;

		protected override string ShaderName
		{
			get
			{
				return SgtHelper.ShaderNamePrefix + "Backdrop";
			}
		}

		public static SgtBackdrop Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBackdrop Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = SgtHelper.CreateGameObject("Backdrop", layer, parent, localPosition, localRotation, localScale);
			var backdrop   = gameObject.AddComponent<SgtBackdrop>();

			return backdrop;
		}

#if UNITY_EDITOR
		[MenuItem(SgtHelper.GameObjectMenuPrefix + "Backdrop", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = SgtHelper.GetSelectedParent();
			var backdrop = Create(parent != null ? parent.gameObject.layer : 0, parent);

			SgtHelper.SelectAndPing(backdrop);
		}
#endif

		protected override void OnEnable()
		{
			base.OnEnable();

			Camera.onPreCull   += CameraPreCull;
			Camera.onPreRender += CameraPreRender;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			Camera.onPreCull   -= CameraPreCull;
			Camera.onPreRender -= CameraPreRender;
		}

		protected override void BuildMaterial()
		{
			base.BuildMaterial();

			if (BlendMode == BlendModeType.Default)
			{
				BuildAdditive();
			}

			if (PowerRgb == true)
			{
				SgtHelper.EnableKeyword("SGT_B", material); // PowerRgb
			}
			else
			{
				SgtHelper.DisableKeyword("SGT_B", material); // PowerRgb
			}
		}

		protected override int BeginQuads()
		{
			SgtHelper.BeginRandomSeed(Seed);

			if (StarColors == null)
			{
				StarColors = SgtHelper.CreateGradient(Color.white);
			}
		
			return StarCount;
		}

		protected virtual void NextQuad(ref SgtBackdropQuad star, int starIndex)
		{
			var position = Random.insideUnitSphere;

			position.y *= 1.0f - Squash;

			star.Variant  = Random.Range(int.MinValue, int.MaxValue);
			star.Color    = StarColors.Evaluate(Random.value);
			star.Radius   = Mathf.Lerp(StarRadiusMin, StarRadiusMax, SgtHelper.Sharpness(Random.value, StarRadiusBias));
			star.Angle    = Random.Range(-180.0f, 180.0f);
			star.Position = position.normalized * Radius;
		}

		protected override void EndQuads()
		{
			SgtHelper.EndRandomSeed();
		}
	
		protected override void BuildMesh(Mesh mesh, int starIndex, int starCount)
		{
			var positions = new Vector3[starCount * 4];
			var colors    = new Color[starCount * 4];
			var coords1   = new Vector2[starCount * 4];
			var indices   = new int[starCount * 6];
			var minMaxSet = false;
			var min       = default(Vector3);
			var max       = default(Vector3);

			for (var i = 0; i < starCount; i++)
			{
				NextQuad(ref SgtBackdropQuad.Temp, starIndex + i);

				var offV     = i * 4;
				var offI     = i * 6;
				var radius   = SgtBackdropQuad.Temp.Radius;
				var uv       = tempCoords[SgtHelper.Mod(SgtBackdropQuad.Temp.Variant, tempCoords.Count)];
				var rotation = Quaternion.FromToRotation(Vector3.back, SgtBackdropQuad.Temp.Position) * Quaternion.Euler(0.0f, 0.0f, SgtBackdropQuad.Temp.Angle);
				var up       = rotation * Vector3.up    * radius;
				var right    = rotation * Vector3.right * radius;

				ExpandBounds(ref minMaxSet, ref min, ref max, SgtBackdropQuad.Temp.Position, radius);

				positions[offV + 0] = SgtBackdropQuad.Temp.Position - up - right;
				positions[offV + 1] = SgtBackdropQuad.Temp.Position - up + right;
				positions[offV + 2] = SgtBackdropQuad.Temp.Position + up - right;
				positions[offV + 3] = SgtBackdropQuad.Temp.Position + up + right;

				colors[offV + 0] =
				colors[offV + 1] =
				colors[offV + 2] =
				colors[offV + 3] = SgtBackdropQuad.Temp.Color;

				coords1[offV + 0] = new Vector2(uv.x, uv.y);
				coords1[offV + 1] = new Vector2(uv.z, uv.y);
				coords1[offV + 2] = new Vector2(uv.x, uv.w);
				coords1[offV + 3] = new Vector2(uv.z, uv.w);

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.vertices  = positions;
			mesh.colors    = colors;
			mesh.uv        = coords1;
			mesh.triangles = indices;
			mesh.bounds    = SgtHelper.NewBoundsFromMinMax(min, max);
		}

		protected virtual void CameraPreCull(Camera camera)
		{
			if (models != null)
			{
				for (var i = models.Count - 1; i >= 0; i--)
				{
					var model = models[i];

					if (model != null)
					{
						model.transform.position = camera.transform.position;

						model.Save(camera);
					}
				}
			}
		}

		protected void CameraPreRender(Camera camera)
		{
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