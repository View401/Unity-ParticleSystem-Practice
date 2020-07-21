Shader "Custom/mainShader"
{
    Properties
    {
        _ColorLow("Color Slow Speed",Color)=(0,0,0.5,1)
		_ColorHigh("Color high Speed",Color)=(1,0,0,1)
		_highSpeedValue("High speed value",Range(0,50))=25
    }
    SubShader
    {
		Pass{
			Blend SrcAlpha one

			CGPROGRAM
			#pragma target 5.0

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct Particle {
				float2 position;
				float2 velocity;     //速度
			};
			
			struct PS_INPUT {
				float4 position:SV_POSITION;
				float4 color:COLOR;
			};
			//Particle被compute shader使用
			StructuredBuffer<Particle> particles;

			uniform float4 _ColorLow;
			uniform float4 _ColorHigh;
			uniform float4 _highSpeedValue;

			PS_INPUT vert(uint vertex_id:SV_VertexID,uint instance_id:SV_InstanceID){
				PS_INPUT o = (PS_INPUT)0;

				//color
				float speed = length(particles[instance_id].velocity);
				float lerpValue = clamp(speed / _highSpeedValue, 0, 1.0);
				o.color = lerp(_ColorLow, _ColorHigh, lerpValue);

				//position
				o.position = UnityObjectToClipPos(float4(particles[instance_id].position, 0.0f, 1.0f));

				return o;
			}

			//pixel shader
			float4 frag(PS_INPUT i):COLOR{
				return i.color;
			}
			ENDCG
		}
    }
		Fallback Off
}
