using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtTerrainSimplex))]
	public class SgtTerrainSimplex_Editor : SgtEditor<SgtTerrainSimplex>
	{
		protected override void OnInspector()
		{
			var updateNoise  = false;
			var dirtyTerrain = false;

			BeginError(Any(t => t.Frequency == 0.0f));
				DrawDefault("Frequency", ref dirtyTerrain, "The density/frequency/tiling of the displacement.");
			EndError();
			BeginError(Any(t => t.Amplitude == 0.0f));
				DrawDefault("Amplitude", ref dirtyTerrain, "The +- strength of the displacement.");
			EndError();
			DrawDefault("Octaves", ref dirtyTerrain, ref updateNoise, "The detail of the simplex noise.");
			DrawDefault("Seed", ref dirtyTerrain, ref updateNoise, "This allows you to set the random seed used during procedural generation.");

			if (updateNoise  == true) DirtyEach(t => t.UpdateNoise ());
			if (dirtyTerrain == true) DirtyEach(t => t.Rebuild());
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component applies a FBM simplex distribution to the current terrain.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtTerrainSimplex")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Terrain Simplex")]
	public class SgtTerrainSimplex : SgtTerrainModifier
	{
		/// <summary>The density/frequency/tiling of the displacement.</summary>
		public double Frequency = 10;

		/// <summary>The +- strength of the displacement.</summary>
		public double Amplitude = 0.5;

		/// <summary>The detail of the simplex noise.</summary>
		[Range(1, 20)]
		public int Octaves = 5;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public SgtSeed Seed;

		[System.NonSerialized]
		private SgtSimplex[] generators;

		[System.NonSerialized]
		private double scale = 1.0;

		public void UpdateNoise()
		{
			if (generators == null || generators.Length != Octaves)
			{
				generators = new SgtSimplex[Octaves];
			}

			var weight = 1.0;
			var total  = 0.0;

			for (var i = 0; i < Octaves; i++)
			{
				var generator = generators[i];

				if (generator == null)
				{
					generator = generators[i] = new SgtSimplex();
				}

				generator.SetSeed(Seed + i * 999);

				total  += weight;
				weight *= 0.5;
			}

			if (total > 0.0f)
			{
				scale = 1.0 / total;
			}
			else
			{
				scale = 1.0;
			}
		}

		protected virtual void OnEnable()
		{
			Rebuild();

			terrain.OnCalculateHeight += CalculateHeight;

			UpdateNoise();
		}

		protected virtual void OnDisable()
		{
			Rebuild();

			terrain.OnCalculateHeight -= CalculateHeight;
		}

		private void CalculateHeight(SgtVector3D localPosition, ref double height)
		{
			localPosition /= localPosition.magnitude;
			localPosition *= Frequency;

			var contribution = 0.0;
			var weight       = 1.0;

			for (var i = 0; i < Octaves; i++)
			{
				localPosition *= 2.0;
				contribution  += generators[i].Generate((float)localPosition.x, (float)localPosition.y, (float)localPosition.z) * weight;
				weight        *= 0.5;
			}

			// Scale maximum constribution to -1..1
			contribution *= scale;

			// Scale and add
			height += contribution * Amplitude;
		}
	}
}