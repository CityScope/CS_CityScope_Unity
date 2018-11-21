Shader "Custom/HeightDependentTint"
{
	Properties
	{
		_MainTex("Base (RGB) Alpha (A)", 2D) = "white" {}

		_HeightMin("Height Min", Float) = -1
		_HeightMax("Height Max", Float) = 1
		_ColorMin("Tint Color At Min", Color) = (0,0,0,0.5)
		_ColorMax("Tint Color At Max", Color) = (1,1,1,0.5)
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }		
		
		Pass{
			ZWrite On
			ColorMask 0
		}
		UsePass "Transparent/Diffuse/FORWARD"


		CGPROGRAM
#pragma surface surf Lambert alpha vertex:vert


		sampler2D _MainTex;
		fixed4 _ColorMin;
		fixed4 _ColorMax;
		float _HeightMin;
		float _HeightMax;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 localPos;
		};

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			//float h = (_HeightMax - IN.localPos.y) / (_HeightMax - _HeightMin); // move from local to world location 
			float h = (_HeightMax - IN.worldPos.y) / (_HeightMax - _HeightMin);
			fixed4 tintColor = lerp(_ColorMax.rgba, _ColorMin.rgba, h);
			o.Albedo = c.rgb * tintColor.rgb;
		 	o.Alpha = tex2D (_MainTex, IN.uv_MainTex).a;
			//o.Alpha = tintColor.a;
			o.Alpha = _ColorMax.a;
		}


		ENDCG
	}
	Fallback "Diffuse"
}