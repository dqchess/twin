Shader "Hidden/SgtBackdrop"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_Color("Color", Color) = (1, 1, 1, 1)

		_SrcMode("Src Mode", Float) = 1 // 1 = One
		_DstMode("Dst Mode", Float) = 1 // 1 = One
		_ZWriteMode("ZWrite Mode", Float) = 8 // 8 = Always
	}
	SubShader
	{
		Tags
		{
			"Queue"           = "Transparent"
			"RenderType"      = "Transparent"
			"IgnoreProjector" = "True"
		}
		Pass
		{
			Blend[_SrcMode][_DstMode]
			ZWrite[_ZWriteMode]
			ZTest LEqual
			Cull Off

			CGPROGRAM
				#pragma vertex Vert
				#pragma fragment Frag
				// Alpha Test
				#pragma multi_compile __ SGT_A
				// RGB Power
				#pragma multi_compile __ SGT_B

				sampler2D _MainTex;
				float4    _Color;

				struct a2v
				{
					float4 vertex    : POSITION;
					float4 color     : COLOR;
					float2 texcoord0 : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex    : SV_POSITION;
					float4 color     : COLOR;
					float2 texcoord0 : TEXCOORD0;
				};

				struct f2g
				{
					float4 color : COLOR;
				};

				void Vert(a2v i, out v2f o)
				{
					o.vertex    = UnityObjectToClipPos(i.vertex);
					o.color     = i.color * _Color;
					o.texcoord0 = i.texcoord0;
				}

				void Frag(v2f i, out f2g o)
				{
					o.color = tex2D(_MainTex, i.texcoord0);
#if SGT_B // RGB Power
					o.color.rgb = pow(o.color.rgb, float3(1.0f, 1.0f, 1.0f) + (1.0f - i.color.rgb) * 10.0f);
#else
					o.color *= i.color;
#endif
					o.color.a = saturate(o.color.a);
					o.color *= i.color.a;
#if SGT_A // Alpha Test
					if (o.color.a < 0.5f)
					{
						o.color.a = 0.0f; discard;
					}
					else
					{
						o.color.a = 1.0f;
					}
#endif
				}
			ENDCG
		} // Pass
	} // SubShader
} // Shader