// This shader is designed specifically for particle effects that require a stylized,
// cel-shaded look. 
Shader "Custom/ToonParticle"
{
    Properties
    {
        [Header(Main Properties)]
        _BaseMap("Particle Texture (Mask)", 2D) = "white" {}
        _BaseColor("Color Tint", Color) = (1,1,1,1)

        [Header(Toon Lighting)]
        _ColorRamp("Lighting Ramp", 2D) = "white" {}

        [Header(Transparency Settings)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 5 // SrcAlpha (Default)
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 1 // One (Default for Additive)
        [Enum(Off, 0, On, 1)] _ZWrite("Depth Write", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            // --- Render States for Particles ---
            // These are controlled by the properties in the Inspector.
            // The default is set to an Additive blend, which is great for energy/magic effects.
            // For smoke/fire, you change Destination Blend to "One Minus Src Alpha".
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Off // Render both sides of a particle quad

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                float4 color        : COLOR; // IMPORTANT: This receives color from the particle system
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldNormal  : TEXCOORD1;
                float4 color        : COLOR; // Pass the particle color to the fragment shader
                float4 shadowCoord  : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
            TEXTURE2D(_ColorRamp);   SAMPLER(sampler_ColorRamp);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
            CBUFFER_END

            // --- Vertex Shader ---
            // Positions vertices and prepares data for the fragment shader.
            Varyings vert(Attributes v)
            {
                Varyings o;
                
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionHCS = posInputs.positionCS;

                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);
                o.worldNormal = normalInputs.normalWS;

                o.shadowCoord = GetShadowCoord(posInputs);

                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.color = v.color; 
                
                return o;
            }

            // --- Fragment Shader ---
            // Calculates the final color for each pixel.
            half4 frag(Varyings i) : SV_Target
            {
                // --- Toon Lighting ---
                Light mainLight = GetMainLight(i.shadowCoord);
                
                half NdotL = saturate(dot(normalize(i.worldNormal), mainLight.direction));

                half3 celColor = SAMPLE_TEXTURE2D(_ColorRamp, sampler_ColorRamp, float2(NdotL, 0.5)).rgb;
                half3 lighting = celColor * mainLight.color * mainLight.shadowAttenuation;

                // --- Texture & Color Sampling ---
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);

                // --- Final Combination ---
                // 1. Start with the texture color
                // 2. Multiply by the particle system's color (from Color over Lifetime, etc.)
                // 3. Multiply by the material's overall tint color
                // 4. Apply the final toon lighting
                half3 finalColor = baseTex.rgb * i.color.rgb * _BaseColor.rgb * lighting;

                half finalAlpha = baseTex.a * i.color.a * _BaseColor.a;

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
}
