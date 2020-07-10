Shader "Pixyz/Splats"
{
   Properties
   {
      [KeywordEnum(ScreenSpace, WorldSpace, PixelSize)] _SizeMode("Size Mode", Float) = 0
      _Size("Size", float) = 0.0035

      _NearZoom("Near Zoom", Range(0, 10)) = 3
      _FarDezoom("Far Dezoom", Range(0, 0.01)) = 0.0025
   }

   SubShader
   {
      Pass
      {
         Name "Splats"

         Tags { "RenderType" = "Opaque" }
         Cull Off

         CGPROGRAM
         #pragma target 4.0
         #pragma vertex vert
         #pragma geometry geom
         #pragma fragment frag
         #include "UnityCG.cginc"
         #pragma multi_compile _SIZEMODE_SCREENSPACE _SIZEMODE_WORLDSPACE _SIZEMODE_PIXELSIZE

         struct appdata
         {
            float4 pos_object : POSITION;
            half3 color : COLOR;
         };

         struct varyings
         {
            float4 pos_clip : SV_POSITION;
            half3 color : COLOR;
         };

         varyings vert(appdata v)
         {
            varyings o;
            o.pos_clip = UnityObjectToClipPos(v.pos_object);
            o.color = v.color;
            return o;
         }

         float _Size;
         float _NearZoom;
         float _FarDezoom;

         [maxvertexcount(6)]
         void geom(point varyings i[1], inout TriangleStream<varyings> output)
         {
            float4 origin = i[0].pos_clip;
            
#if defined(_SIZEMODE_WORLDSPACE)
            float2 extent = abs(UNITY_MATRIX_P._11_22 * _Size); // Fixed world size
#elif defined(_SIZEMODE_SCREENSPACE)
            float2 extent = abs(UNITY_MATRIX_P._11_22 * _Size) * origin.w; // Fixed screen height size
#elif defined(_SIZEMODE_PIXELSIZE)
            float2 extent = abs(UNITY_MATRIX_P._11_22 * _Size) * origin.w / _ScreenParams.y; // Fixed pixel size
#endif
            extent *= 1 + _NearZoom * exp(-origin.w) - (2 / UNITY_PI) * atan(_FarDezoom * origin.w);

            varyings o = i[0];

            o.pos_clip.y = origin.y + extent.y;
            o.pos_clip.xzw = origin.xzw;
            output.Append(o);

            float sn, cs;

            [unroll] for (int i = 1; i < 3; i++)
            {
               sincos(UNITY_PI / 3 * i, sn, cs);

               o.pos_clip.xy = origin.xy + extent * float2(sn, cs);
               output.Append(o);

               o.pos_clip.x = origin.x - extent.x * sn;
               output.Append(o);
            }

            o.pos_clip.x = origin.x;
            o.pos_clip.y = origin.y - extent.y;
            output.Append(o);

            output.RestartStrip();
         }

         half4 frag(varyings i) : SV_Target
         {
            return half4(i.color.rgb, 1);
         }

         ENDCG
      }
   }
}