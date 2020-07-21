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
				float4 vector:POSITION;
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
			float4x4 GetModelToWorldMatrix(float3 pos)
			{
				float4x4 transformMatrix = float4x4(
					_Size, 0, 0, pos.x,
					0, _Size, 0, pos.y,
					0, 0, _Size, pos.z,
					0, 0, 0, 1
					);
				return transformMatrix;
			}
            // Particle's data, shared with the compute shader
            StructuredBuffer<particleCacheProperty> Particles;
            float _size;
            // Properties variables
            /*uniform float4 cbStartColor;
            uniform float4 cbEndColor;
            uniform float cbStartSize;
			uniform float cbEndSize;
			uniform float cbParticlesStorage;
			uniform float cbLifeDecay;*/

            // Vertex shader
            PS_INPUT vert(appdata v, uint instance_id : SV_InstanceID)
            {
				PS_INPUT o = (PS_INPUT)0;

                // Color
                //float speed = length(Particles[instance_id].velocity);
                //float lerpValue = clamp(speed / _HighSpeedValue, 0.0f, 1.0f);
                o.color = Particles[instance_id].color;
                // Position
                //o.position = UnityObjectToClipPos(float4(Particles[instance_id].position.xyz,1.0));
				float4x4 WorldMatrix = GetModelToWorldMatrix(Particles[instance_id].position.xyz);
				v.vertex = mul(WorldMatrix, v.vertex);
				o.position = mul(UNITY_MATRIX_VP, v.vertex);
				//o.initialVelocity = Particles[instance_id].initialVelocity;
				//o.initialPosition = Particles[instance_id].initialPosition;

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
