// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Pixyz/Thick Lines"
{

	Properties
	{
		_Color("Color", Color) = (1,1,1,1)

		_Size("Thickness", Range(0.001, 0.005)) = 0.0020

		[Toggle]
		_RelScreen("Relative to Screen", Float) = 0

		[Toggle]
		_Offset("See-Through", Float) = 0

		[Header(Dashes)]
		[Toggle]
		_Dashes("Enabled", Float) = 0
		_DashesScale("Scale", Range(1, 10)) = 1.0
	}
		SubShader
	{
		 //Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		 Tags { "RenderType" = "Opaque" }
		 Cull Off

		 Pass
		 {
			  CGPROGRAM
			  #pragma target 5.0
			  #pragma geometry geom
			  #pragma vertex vert
			  #pragma fragment frag
			  #include "UnityCG.cginc"

			  // Vars
			  float _Size;
			  float3 _worldPos;
			  float _RelScreen;
			  float _Offset;
			  
			  struct appdata
			  {
				  float4 position : POSITION;
			  };

			  struct input
			  {
					float4 position : POSITION;
					float3 coords : TEXCOORD1;
			  };

			  input vert(appdata i)
			  {
					input o;
					o.position = UnityObjectToClipPos(i.position);
					if (unity_CameraProjection._m33 == 0) {
						o.position.z *= 1 + 0.004 * (1 + _Offset * 1000) / unity_CameraProjection._m11;
					}
					o.coords = i.position.xyz;
					return o;
			  }

			  [maxvertexcount(16)]
			  void geom(line input p[2], inout TriangleStream<input> triStream)
			  {
					float4 s[2];

					s[0] = p[0].position;
					s[1] = p[1].position;

					s[0].x /= s[0].w;
					s[0].y /= s[0].w;
					s[1].x /= s[1].w;
					s[1].y /= s[1].w;

					s[0].z /= s[0].w;
					s[1].z /= s[1].w;
 
					s[0].w = s[1].w = 1;

					float4 ab = s[1] - s[0];
					float4 normal0 = normalize(float4(-ab.y, ab.x, 0, 0));
					normal0.x /= (_ScreenParams.x / _ScreenParams.y);
					float4 normal1 = normal0;

					if (_RelScreen > 0) {
						normal0 /= clamp(distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, p[0].position)), .01, 2.0);
						normal1 /= clamp(distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, p[1].position)), .01, 2.0);
					}
				
					input pIn;

					pIn.position = s[0] - normal0 * _Size;
					pIn.coords = p[0].position;
					triStream.Append(pIn);

					pIn.position = s[0] + normal0 * _Size;
					pIn.coords = p[0].position;
					triStream.Append(pIn);

					pIn.position = s[1] - normal1 * _Size;
					pIn.coords = p[1].position;
					triStream.Append(pIn);

					pIn.position = s[1] + normal1 * _Size;
					pIn.coords = p[1].position;
					triStream.Append(pIn);
			  }

			  bool checkerboard(int x, int y, int z)
			  {
				  return (fmod(x, 2) == 0) ^ (fmod(y, 2) == 0) ^ (fmod(z, 2) == 0);
			  }

			  float4 _Color;
			  float _Dashes;
			  float _DashesScale;

			  fixed4 frag(input i) : COLOR
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

	FallBack "Diffuse"
}