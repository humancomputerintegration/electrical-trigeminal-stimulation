Shader "Pixyz/Simple Lines"
{
	Properties
	{
		[Header(Main Properties)]
		[HDR]
		_Color ("Color", Color) = (0,0,0,1)

		[Toggle]
		_Offset ("See-Through", Float) = 0

		[Header(Dashes)]
		[Toggle]
		_Dashes ("Enabled", Float) = 0
      _DashesScale ("Scale", Range(1, 10)) = 1.0
	}
	SubShader
	{
		Tags { "Queue" = "Overlay" }
		LOD 200

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
			};

			struct input
			{
				float4 position : POSITION;
				float3 coords : TEXCOORD1;
			};

			float _Offset;

			input vert (appdata i)
			{
				input o;
				o.position = UnityObjectToClipPos(i.position);
				if (unity_CameraProjection._m33 == 0) {
					o.position.z *= 1 + 0.004 * (1 + _Offset * 1000) / unity_CameraProjection._m11;
				}
				o.coords = i.position.xyz;
				return o;
			}

			fixed4 _Color;
			float _Dashes;
			float _DashesScale;

			bool checkerboard(int x, int y, int z)
			{
				return (fmod(x, 2) == 0) ^ (fmod(y, 2) == 0) ^ (fmod(z, 2) == 0);
			}

			fixed4 frag (input i) : SV_Target
			{
				if (_Dashes) {
					i.coords *= int(1000 / (_DashesScale + 0.001));
					if (checkerboard(i.coords.x, i.coords.y, i.coords.z)) {
						discard;
					}
				}
            return _Color;
			}
			ENDCG
		}
	}
}
