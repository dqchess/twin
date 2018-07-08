Shader "Hidden/SgtCoronaInner"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_DepthTex("Depth Tex", 2D) = "white" {}
		_HorizonLengthRecip("Horizon Length Recip", Float) = 0
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
			Blend SrcAlpha OneMinusSrcAlpha, One One
			Cull Back
			Lighting Off
			Offset -0.1, 0
			ZWrite Off

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			// Outside
			#pragma multi_compile __ SGT_A

			float4    _Color;
			sampler2D _DepthTex;
			float     _HorizonLengthRecip;
			float4x4  _WorldToLocal;

			struct a2v
			{
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex    : SV_POSITION;
				float2 texcoord0 : TEXCOORD0; // 0..1 depth
			};

			struct f2g
			{
				float4 color : COLOR;
			};

			float3 IntersectUnitSphere(float3 ray, float3 rayD)
			{
				float B = dot(ray, rayD);
				float C = dot(ray, ray) - 1.0f;
				float D = B * B - C;
				return ray + (-B - sqrt(D)) * rayD;
			}

			void Vert(a2v i, out v2f o)
			{
				float4 wPos = mul(unity_ObjectToWorld, i.vertex);
				float4 far  = mul(_WorldToLocal, wPos);
				float4 near = mul(_WorldToLocal, float4(_WorldSpaceCameraPos, 1.0f));
#if SGT_A // Outside
				near.xyz = IntersectUnitSphere(far.xyz, normalize(far.xyz - near.xyz));
#endif
				float depth   = length(near.xyz - far.xyz);
				float horizon = depth * _HorizonLengthRecip;

				o.vertex    = UnityObjectToClipPos(i.vertex);
				o.texcoord0 = horizon;
			}

			void Frag(v2f i, out f2g o)
			{
				float4 depth = tex2D(_DepthTex, i.texcoord0);
				float4 main = depth * _Color;

				o.color = main;
			}
			ENDCG
		} // Pass
	} // SubShader
} // Shader