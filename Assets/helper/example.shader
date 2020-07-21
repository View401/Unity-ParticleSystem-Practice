Shader "Unlit/example"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MainTex2("Texture2", 2D) = "yello" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //LOD 100
		LOD 200
        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members uv)
#pragma exclude_renderers d3d11
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members uv)
//#pragma exclude_renderers d3d11
			#pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

			struct Particle {
				float3 position;
				float3 CustomPos;
				float3 color;
				float2 uv;
			};
			StructuredBuffer<Particle> particles;
			sampler2D _MainTex, _MainTex2;
			
			float size;
			
			
			float lerpt;
			float4x4 GameobjectMatrix;
            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float4 pos:SV_POSITION;
				float2 uv :TEXCOORD0;
            };

			float4x4 GetModelToWorldMatrix(float3 pos) {
				float4x4 transformMatrix = float4x4(
					size, 0, 0, pos.x,
					0, size, 0, pos.y,
					0, 0, size, pos.z,
					0, 0, 0, 1
					);
				return transformMatrix;
			}
            v2f vert (appdata v, uint instance_id : SV_InstanceID)
            {
                v2f o;
				//
				float4x4 WorldMatrix = GetModelToWorldMatrix(particles[instance_id].position.xyz);
				WorldMatrix = mul(GameobjectMatrix, WorldMatrix);
				v.vertex = mul(WorldMatrix, v.vertex);
				//o.pos = UnityObjectToClipPos(float4(particles[instance_id].position,1.0));
				o.pos = mul(UNITY_MATRIX_VP, v.vertex);
				o.uv = particles[instance_id].uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
				fixed4 col = tex2D(_Maintex,i.uv);
				fixed4 col2 = tex2D(_Maintex2, i.uv);
				//col = mix(col, col2, lerpt);
				col = lerp(col, col2, lerpt);
                return col;
            }
            ENDCG
        }
    }
		FallBack Off
}
