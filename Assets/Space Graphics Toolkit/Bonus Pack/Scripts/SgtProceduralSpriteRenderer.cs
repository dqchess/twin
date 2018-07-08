using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace SpaceGraphicsToolkit
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtProceduralSpriteRenderer))]
	public class SgtProceduralSpriteRenderer_Editor : SgtEditor<SgtProceduralSpriteRenderer>
	{
		protected override void OnInspector()
		{
			DrawDefault("Colors", "A color will be randomly picked from this gradient.");
			DrawDefault("UseFloatingObject", "If you enable this then the procedural generation to be based on the SgtFloatingObject.Seed.");
			if (Any(t => t.UseFloatingObject == true && t.GetComponent<SgtFloatingObject>() == null))
			{
				EditorGUILayout.HelpBox("Your GameObject doesn't have the SgtFloatingObject component.", MessageType.Error);
			}
		}
	}
}
#endif

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to procedurally generate the SpriteRenderer.color setting.</summary>
	[RequireComponent(typeof(SpriteRenderer))]
	[HelpURL(SgtHelper.HelpUrlPrefix + "SgtProceduralSpriteRenderer")]
	[AddComponentMenu(SgtHelper.ComponentMenuPrefix + "Procedural Sprite Renderer")]
	public class SgtProceduralSpriteRenderer : MonoBehaviour
	{
		/// <summary>A color will be randomly picked from this gradient.</summary>
		public Gradient Colors;

		/// <summary>If you enable this then the procedural generation to be based on the SgtFloatingObject.Seed.</summary>
		public bool UseFloatingObject;

		[System.NonSerialized]
		private SgtFloatingObject cachedFloatingObject;

		[ContextMenu("Generate")]
		public void Generate()
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();

			spriteRenderer.color = Colors.Evaluate(Random.value);
		}

		protected virtual void OnEnable()
		{
			if (UseFloatingObject == true)
			{
				cachedFloatingObject = GetComponent<SgtFloatingObject>();

				cachedFloatingObject.OnSpawn += SpawnSeed;
			}
		}

		protected virtual void OnDisable()
		{
			if (UseFloatingObject == true)
			{
				cachedFloatingObject.OnSpawn -= SpawnSeed;
			}
		}

		private void SpawnSeed(int seed)
		{
			SgtHelper.BeginRandomSeed(seed);
			{
				Generate();
			}
			SgtHelper.EndRandomSeed();
		}
	}
}