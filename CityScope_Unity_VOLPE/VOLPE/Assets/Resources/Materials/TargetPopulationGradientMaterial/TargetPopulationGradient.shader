// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TargetPopulationGradient" {
     Properties {

    	   _MaxEffectArea ("Maximum radius of green gradient of a target", float) = 100.0
      

     }
     SubShader {
         Tags { "RenderType"="Transparent" "Queue"="Transparent"}
         Pass {
         Blend SrcAlpha OneMinusSrcAlpha
         LOD 200		
     
         CGPROGRAM
         #pragma vertex vert
         #pragma fragment frag

         uniform float _MaxEffectArea;
         uniform float _TargetValues[256];
         uniform float3 _TargetPositions[256];


         // Input to vertex shader
         struct vertexInput {
             float4 vertex : POSITION;
             float4 texcoord : TEXCOORD0;
          };
         // Input to fragment shader
          struct vertexOutput {
             float4 pos : SV_POSITION;
             float4 position_in_world_space : TEXCOORD0;
             float4 tex : TEXCOORD1;
          };

          float3 hsv2rgb(float r, float g, float b) {
          	  float3 c;
          	  c.r = r;
          	  c.g = g;
          	  c.b = b;
		      float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
		      float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
		      return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
		  }

		  float3 rgb2hsv(float3 c) {
		  	  float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			  float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
			  float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

			  float d = q.x - min(q.w, q.y);
			  float e = 1.0e-10;
			  return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		  }
          
          // VERTEX SHADER
          vertexOutput vert(vertexInput input) 
          {
             vertexOutput output; 
             output.pos =  UnityObjectToClipPos(input.vertex);
             output.position_in_world_space = mul(unity_ObjectToWorld, input.vertex);
             output.tex = input.texcoord;
             return output;
          }
  
          // FRAGMENT SHADER
         float4 frag(vertexOutput input) : COLOR 
         {
             // Calculate distance to player position
  			 float4 output_texture;
  			 float gradient = 0.0;
  			 for (int i=0; i<256; i++) {
  			 	float radius_ratio = (_TargetValues[i] * _MaxEffectArea - distance(input.position_in_world_space, _TargetPositions[i])) / _MaxEffectArea;
  			 	if (radius_ratio < 0) radius_ratio = 0;
  			 	gradient += _TargetValues[i] * radius_ratio;
  			 }
  			 float3 rgb_color = hsv2rgb(gradient / 5, .99, .8);

  			 output_texture.g = rgb_color.g;
  			 output_texture.r = rgb_color.r;
  			 output_texture.b = rgb_color.b;
  			 output_texture.a = 0.8;

             return output_texture;
          }
 
         ENDCG
         }
     } 
     //FallBack "Diffuse"
 }