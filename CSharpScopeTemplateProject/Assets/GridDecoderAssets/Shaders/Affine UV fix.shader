// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Affine UV fix Cg" {
 
    Properties {
 
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }
 
    SubShader {

    	Lighting Off
        Tags { "RenderType"="Opaque" }
		LOD 100
         
        pass{
            CGPROGRAM
 
            uniform sampler2D _MainTex;
         
            #pragma vertex vert          
            #pragma fragment frag
         
 
	        struct vertexInput {
	            float4 vertex : POSITION;        
	            float3 texcoord  : TEXCOORD0;
	            float2 texcoord1 : TEXCOORD1;
	        };
	 
	        struct vertexOutput {
	            float4 pos : SV_POSITION;
	            float3 uv  : TEXCOORD0;
	            float2 uv2 : TEXCOORD1;
	        };
	 
	        vertexOutput vert(vertexInput input)
	        {
	            vertexOutput output;

	            output.pos = UnityObjectToClipPos (input.vertex);
	         
	            output.uv = input.texcoord;
	            output.uv2 = input.texcoord1;
	         
	            return output;
	        }
	 
	        float4 frag(vertexOutput input) : COLOR
	        {    
	            float4 startColor =  tex2D(_MainTex, float2(input.uv.xy)/(input.uv.z));
	            // Adjust brightness / contrast
	            startColor = (startColor - 0.5f) * max(input.uv2.x, 0) + input.uv2.y;
                return startColor;
	        }
     
         ENDCG // here ends the part in Cg
    	}
    }
 
}