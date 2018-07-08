using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This base class handles calculation of a shadow matrix and shadow texture.</summary>
	public abstract class SgtShadow : SgtLinkedBehaviour<SgtShadow>
	{
		private static List<ShadowProperties> cachedShadowProperties = new List<ShadowProperties>();

		private static List<string> cachedShadowKeywords = new List<string>();

		public abstract Texture GetTexture();

		public abstract bool CalculateShadow(ref Matrix4x4 matrix, ref float ratio);

		private class ShadowProperties
		{
			public int Texture;
			public int Matrix;
			public int Ratio;
		}

		private static ShadowProperties GetShadowProperties(int index)
		{
			for (var i = cachedShadowProperties.Count; i <= index; i++)
			{
				var properties = new ShadowProperties();
				var prefix     = "_Shadow" + (i + 1);

				properties.Texture = Shader.PropertyToID(prefix + "Texture");
				properties.Matrix  = Shader.PropertyToID(prefix + "Matrix");
				properties.Ratio   = Shader.PropertyToID(prefix + "Ratio");

				cachedShadowProperties.Add(properties);
			}

			return cachedShadowProperties[index];
		}

		private static string GetShadowKeyword(int index)
		{
			for (var i = cachedShadowKeywords.Count; i <= index; i++)
			{
				cachedShadowKeywords.Add("SHADOW_" + i);
			}

			return cachedShadowKeywords[index];
		}

		public static void Write(bool lit, GameObject root, int maxShadows)
		{
			var shadowCount = 0;

			if (lit == true)
			{
				var light = default(SgtLight);

				if (SgtLight.Find(ref light) == true)
				{
					var shadow = FirstInstance;

					for (var i = 0; i < InstanceCount; i++)
					{
						if (shadow.gameObject != root)
						{
							var matrix = default(Matrix4x4);
							var ratio  = default(float);

							if (shadow.CalculateShadow(ref matrix, ref ratio) == true)
							{
								var properties = GetShadowProperties(shadowCount++);

								for (var j = SgtHelper.tempMaterials.Count - 1; j >= 0; j--)
								{
									var tempMaterial = SgtHelper.tempMaterials[j];

									if (tempMaterial != null)
									{
										tempMaterial.SetTexture(properties.Texture, shadow.GetTexture());
										tempMaterial.SetMatrix(properties.Matrix, matrix);
										tempMaterial.SetFloat(properties.Ratio, ratio);
									}
								}
							}

							if (shadowCount >= maxShadows)
							{
								break;
							}
						}

						shadow = shadow.NextInstance;
					}
				}
			}

			for (var i = 0; i <= maxShadows; i++)
			{
				var keyword = GetShadowKeyword(i);

				if (lit == true && i == shadowCount)
				{
					SgtHelper.EnableKeyword(keyword);
				}
				else
				{
					SgtHelper.DisableKeyword(keyword);
				}
			}
		}
		/*
		public static void WriteShadowsNonSerialized(List<SgtShadow> shadows, int maxShadows)
		{
			if (shadows != null)
			{
				var shadowCount = 0;

				for (var i = 0; i < shadows.Count && shadowCount < maxShadows; i++)
				{
					var shadow = shadows[i];
					var matrix = default(Matrix4x4);
					var ratio  = default(float);

					if (Enabled(shadow) == true && shadow.CalculateShadow(ref matrix, ref ratio) == true)
					{
						var prefix = "_Shadow" + (++shadowCount);

						SetMatrix(prefix + "Matrix", matrix);
					}
				}
			}
		}
		*/
	}
}