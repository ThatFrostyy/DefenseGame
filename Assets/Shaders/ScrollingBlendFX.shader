// This shader blends two textures. The "SubTexture" scrolls over the main "Texture".
Shader "Custom/ScrollingBlendFX"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ColorMod ("ColorMod", Float) = 1
        _AlphaMod ("AlphaMod", Float) = 1

        _MainTex ("Texture", 2D) = "white" {}
        _SubTex ("SubTexture", 2D) = "gray" {}

        _USpeed ("USpeed", Float) = 0.05
        _VSpeed ("VSpeed", Float) = 0.02
    }
    SubShader
    {
        // Set up for transparency
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            // Standard alpha blending
            Blend SrcAlpha OneMinusSrcAlpha
            // Don't write to the depth buffer, important for transparent objects
            ZWrite Off
            // Render both sides of the object, useful for particles/planes
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvMain : TEXCOORD0;
                float2 uvSub : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
            };

            // Properties from the Inspector
            sampler2D _MainTex;
            float4 _MainTex_ST; // _ST is for Tiling & Offset
            sampler2D _SubTex;
            float4 _SubTex_ST;

            float _USpeed;
            float _VSpeed;
            fixed4 _Color;
            float _ColorMod;
            float _AlphaMod;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Pass UVs to the fragment shader, applying Tiling/Offset
                o.uvMain = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvSub = TRANSFORM_TEX(v.uv, _SubTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Create a copy of the sub-texture UVs to modify
                float2 scrollingUV = i.uvSub;
                // Add scrolling based on time and the speed properties
                scrollingUV.x += _Time.y * _USpeed;
                scrollingUV.y += _Time.y * _VSpeed;

                // Sample both textures
                fixed4 mainTexColor = tex2D(_MainTex, i.uvMain);
                fixed4 subTexColor = tex2D(_SubTex, scrollingUV);

                // Combine the textures by multiplying them.
                fixed4 finalColor = mainTexColor * subTexColor;

                // Apply the tint color
                finalColor *= _Color;

                // Apply the color and alpha modifiers
                finalColor.rgb *= _ColorMod;
                finalColor.a *= _AlphaMod;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }
            ENDCG
        }
    }
}
