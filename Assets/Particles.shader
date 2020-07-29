// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Subway/Particles"
{

	//Properties
	//{
		//cbStartColor("start color", Color) = (0, 0, 0.5, 1)
		//cbEndColor("end color", Color) = (1, 0, 0, 1)
		////_HighSpeedValue("High speed Value", Range(0, 50)) = 25
		//cbStartSize("start size", Float) = 0.5
		//cbEndSize("end size", Float) = 2.5
		//cbParticlesStorage("storage", Int) = 10000
		//cbLifeDecay("life decay", Float) = 0.003
    //}

    SubShader
    {
        Pass
        {
			Tags{"RenderType" = "Opaque"}
            Blend SrcAlpha one
			
            CGPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Particle's data
			struct particleCacheProperty {
				float4 position;
				float4 color;
				float4 initialPosition;
				float4 initialVelocity;
				
				float4 velocity;
				
				//float4 ambient;
				float2 ageAndlife;
				float psize;
				//float2 massAndSize;
				//float2 padding;

			};
			struct appdata {
				float4 vertex:POSITION;
			};
            // Pixel shader input
            struct PS_INPUT
            {
				//float4 initialPosition:TEXCOORD0;
				//float4 initialVelocity:TEXCOORD1;
                float4 position : SV_POSITION;
				//float4 velocity: TEXCOORD2;
                float4 color : COLOR;
				float size : PSIZE;
				//float4 ambient:TEXCOORD3;
				//float2 ageAndlife:TEXCOORD4;
            };
			float _size;
			float4x4 GetModelToWorldMatrix(float3 pos)
			{
				float4x4 transformMatrix = float4x4(
					_size, 0, 0, pos.x,
					0, _size, 0, pos.y,
					0, 0, _size, pos.z,
					0, 0, 0, 1
					);
				return transformMatrix;
			}
            // Particle's data, shared with the compute shader
            StructuredBuffer<particleCacheProperty> Particles;

            // Vertex shader
            PS_INPUT vert(appdata v, uint instance_id : SV_InstanceID)
            {
				PS_INPUT o = (PS_INPUT)0;

				particleCacheProperty particle = Particles[instance_id];
                // Color
                o.color = particle.color;
                // Position
                //o.position = UnityObjectToClipPos(float4(Particles[instance_id].position.xyz,1.0));
				_size = particle.psize;
				float4x4 WorldMatrix = GetModelToWorldMatrix(particle.position.xyz);
				
				float3 objViewDir = -mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
				float3 normalDir = normalize(objViewDir);
				float3 upDir = float3(0, 1, 0);
				float3 rightDir = normalize(cross(normalDir, upDir));
				upDir = normalize(cross(normalDir, rightDir));
				float3 localPos = rightDir * v.vertex.x + upDir * v.vertex.y + normalDir * v.vertex.z;
				o.position= mul(WorldMatrix, float4(localPos, 1));
				o.position = mul(UNITY_MATRIX_VP,o.position);

                return o;
            }

            // Pixel shader
            float4 frag(PS_INPUT i) : COLOR
            {
                return i.color;
            }

            ENDCG
        }
    }

    Fallback Off
}
