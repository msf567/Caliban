// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Fingerhut/PixelParticle" {
    Properties{
		_Color ("Color", Color) = (1,1,1,1)
    }
    SubShader {

    Tags {"RenderType"="Particle"}
    Pass {
        LOD 200
         ZTest Less
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
 
        struct VertexInput {
            float4 v : POSITION;
            float4 color: COLOR;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
            float4 col : COLOR;
        };

		fixed4 _Color;
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            o.pos = UnityObjectToClipPos(v.v);
            o.col = v.color * _Color;
             
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return o.col;
        }
 
        ENDCG
        } 
    }

  SubShader {
  Tags {"RenderType"="Opaque"}
    Pass {
        LOD 200
         ColorMask 0
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
 
        struct VertexInput {
            float4 v : POSITION;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
        };
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            o.pos = UnityObjectToClipPos(v.v);
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return fixed4(0,0,0,0);
        }
 
        ENDCG
        } 
    }


  SubShader {
  Tags {"RenderType"="Emissive"}
    Pass {
        LOD 200
         ColorMask 0
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
 
        struct VertexInput {
            float4 v : POSITION;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
        };
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
            o.pos = UnityObjectToClipPos(v.v);
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return fixed4(0,0,0,0);
        }
 
        ENDCG
        } 
    }
}