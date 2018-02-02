// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Affine UV fix Cg" {
 
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Brightness ("Brightness", Range(0,1)) = 0.0
		_Contrast ("Contrast", Range(0,2)) = 1.0
    }
 
    SubShader {

    	Lighting Off
        Tags { "RenderType"="Opaque" }
		LOD 100
         
        pass{
            CGPROGRAM
 
            uniform sampler2D _MainTex;

            half _Brightness;
			half _Contrast;
         
            #pragma vertex vert          
            #pragma fragment frag
         
 
	        struct vertexInput {
	            float4 vertex : POSITION;        
	            float3 texcoord  : TEXCOORD0;
	        };
	 
	        struct vertexOutput {
	            float4 pos : SV_POSITION;
	            float3 uv  : TEXCOORD0;
	        };
	 
	        vertexOutput vert(vertexInput input)
	        {
	            vertexOutput output;

	            output.pos = UnityObjectToClipPos (input.vertex);
	         
	            output.uv = input.texcoord;
	         
	            return output;
	        }
	 
	        float4 frag(vertexOutput input) : COLOR
	        {    
	            float4 startColor =  tex2D(_MainTex, float2(input.uv.xy)/(input.uv.z));
	            startColor = (startColor - 0.5f) * max(_Contrast, 0) + 0.5f + _Brightness;
                return startColor;
	        }
     
         ENDCG // here ends the part in Cg
    	}
    }
 
}