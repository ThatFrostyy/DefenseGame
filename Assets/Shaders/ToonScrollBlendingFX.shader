// This shader combines toon/cel-shading with a scrolling, blended texture effect.
Shader "Custom/ToonScrollBlendingFX"
{
    Properties
    {
        [Header(Main Properties)]
        _BaseMap("Main Texture (Mask)", 2D) = "white" {}
        _SubTex("Scrolling Texture", 2D) = "gray" {}
        _BaseColor("Color Tint", Color) = (1,1,1,1)

        [Header(Scrolling)]
        _USpeed("Scroll Speed U (X)", Float) = 0.05
        _VSpeed("Scroll Speed V (Y)", Float) = 0.02

        [Header(Cel Shading)]
        _ColorRamp("Lighting Ramp", 2D) = "white" {}

        [Header(Transparency Settings)]
        [Enum(Opaque, 0, Transparent, 1)] _Surface("Surface Type", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 1 // One
        [Enum(Off, 0, On, 1)] _ZWrite("Depth Write", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }

        // Pass 1: Main Rendering Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            // Render states for transparency
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Off // For particle effects

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR; // For particle system color
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uvMain       : TEXCOORD0;
                float2 uvSub        : TEXCOORD1;
                float3 worldNormal  : TEXCOORD2;
                float4 shadowCoord  : TEXCOORD3;
                float4 color        : COLOR;
            };

            TEXTURE2D(_BaseMap);       SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SubTex);        SAMPLER(sampler_SubTex);
            TEXTURE2D(_ColorRamp);     SAMPLER(sampler_ColorRamp);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _SubTex_ST;
                half4 _BaseColor;
                half _USpeed;
                half _VSpeed;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionHCS = posInputs.positionCS;

                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);
                o.worldNormal = normalInputs.normalWS;

                o.shadowCoord = GetShadowCoord(posInputs);

                o.uvMain = TRANSFORM_TEX(v.uv, _BaseMap);
                o.uvSub = TRANSFORM_TEX(v.uv, _SubTex);
                o.color = v.color; // Pass particle color through
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // --- Toon Lighting Calculation ---
                Light mainLight = GetMainLight(i.shadowCoord);
                half3 normal = normalize(i.worldNormal);
                half NdotL = saturate(dot(normal, mainLight.direction));
                half3 celColor = SAMPLE_TEXTURE2D(_ColorRamp, sampler_ColorRamp, float2(NdotL, 0.5)).rgb;
                half3 lighting = celColor * mainLight.color * mainLight.shadowAttenuation;

                // --- Scrolling Texture Logic ---
                float2 scrollingUV = i.uvSub;
                scrollingUV.x += _Time.y * _USpeed;
                scrollingUV.y += _Time.y * _VSpeed;

                half4 mainTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uvMain);
                half4 subTexColor = SAMPLE_TEXTURE2D(_SubTex, sampler_SubTex, scrollingUV);
                
                // --- Combination ---
                half4 blendedTex = mainTexColor * subTexColor;
                
                // Combine everything: texture color * particle color * tint color * toon lighting
                half3 finalColor = blendedTex.rgb * i.color.rgb * _BaseColor.rgb * lighting;

                // Combine alpha values
                half finalAlpha = blendedTex.a * i.color.a * _BaseColor.a;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}