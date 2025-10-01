// Gemini - WW2 Toon Lit Shader for URP
// This shader provides a stylized "toon" or "cel-shaded" look.
// Features:
// 1. Hard-edged cel-shading based on a ramp texture.
// 2. Configurable outline for model clarity.
// 3. Optional rim lighting to help units stand out.
// 4. Integrates with URP's main directional light and shadows.
// 5. Correctly casts and receives shadows in Unity 6.
// 6. Supports transparent surface type for effects.

Shader "Custom/Toon"
{
    Properties
    {
        [Header(Main Properties)]
        _BaseMap("Base Map (Albedo)", 2D) = "white" {}
        _BaseColor("Base Color Tint", Color) = (1,1,1,1)

        [Header(Cel Shading)]
        _ColorRamp("Lighting Ramp", 2D) = "white" {}
        _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.5

        [Header(Outline)]
        _OutlineColor("Outline Color", Color) = (0.1, 0.1, 0.1, 1)
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.01

        [Header(Rim Lighting)]
        _RimColor("Rim Light Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.1, 10)) = 2.0
        _RimToggle("Enable Rim Light", Float) = 1.0 // Use 0 to disable, 1 to enable
        
        [Header(Transparency Settings)]
        [Enum(Opaque, 0, Transparent, 1)] _Surface("Surface Type", Float) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0
        [Enum(Off, 0, On, 1)] _ZWrite("Depth Write", Float) = 1
    }
    SubShader
    {
        // This shader is for URP. It won't work in the Built-in RP.
        Tags { "RenderPipeline" = "UniversalPipeline" }

        //-------------------------------------------------------------------------------------
        // Pass 1: Main Rendering Pass
        // This pass draws the model with the toon lighting effect.
        //-------------------------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            // Render states for blending and depth. These are controlled by the material properties.
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            
            // Standard render states
            Cull Back
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Pragmas for compiling shadow variants. Necessary for receiving shadows.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            // Includes URP core libraries for lighting and transformations.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Structs to pass data between CPU and GPU stages.
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 worldNormal  : TEXCOORD1;
                float3 viewDir      : TEXCOORD2;
                float4 shadowCoord  : TEXCOORD3; // Added to pass shadow data to fragment shader
            };

            // Define the properties we set in the Inspector.
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_ColorRamp);
            SAMPLER(sampler_ColorRamp);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _OutlineColor;
                half _OutlineWidth;
                half4 _RimColor;
                half _RimPower;
                half _ShadowThreshold;
                half _RimToggle;
            CBUFFER_END


            // VERTEX SHADER: Processes each vertex of the model.
            Varyings vert(Attributes v)
            {
                Varyings o;
                // Transform position and normal from object space to world space, then to screen space.
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionHCS = posInputs.positionCS;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normalOS);
                o.worldNormal = normalInputs.normalWS;
                
                // Calculate shadow coordinates for receiving shadows.
                o.shadowCoord = GetShadowCoord(posInputs);

                // Pass UV coordinates, transformed for tiling/offset.
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);

                // Calculate view direction for rim lighting.
                o.viewDir = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                
                return o;
            }

            // FRAGMENT SHADER: Processes each pixel on the model's surface.
            half4 frag(Varyings i) : SV_Target
            {
                // Get the main light data from URP, including shadow attenuation.
                Light mainLight = GetMainLight(i.shadowCoord);

                // Normalize the world normal (interpolation can change its length).
                i.worldNormal = normalize(i.worldNormal);

                // Calculate the dot product between the surface normal and the light direction.
                // This tells us how much the surface is facing the light.
                // saturate() clamps the value between 0 and 1.
                half NdotL = saturate(dot(i.worldNormal, mainLight.direction));

                // --- Cel Shading Logic ---
                // We sample a ramp texture based on the light intensity (NdotL).
                half2 rampUV = float2(NdotL, 0.5); // Use NdotL as the horizontal coordinate on the ramp
                half3 celColor = SAMPLE_TEXTURE2D(_ColorRamp, sampler_ColorRamp, rampUV).rgb;
                
                // Combine the light color with our cel-shaded result and apply real-time shadows.
                half3 lighting = celColor * mainLight.color * mainLight.shadowAttenuation;

                // --- Texture and Color ---
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
                half3 finalColor = baseTex.rgb * _BaseColor.rgb * lighting;

                // --- Rim Lighting Logic ---
                // Calculates a highlight on the edges of the model, separating it from the background.
                if (_RimToggle > 0.5)
                {
                    half rimDot = 1 - saturate(dot(i.worldNormal, i.viewDir));
                    half rimIntensity = pow(rimDot, _RimPower);
                    finalColor += _RimColor.rgb * rimIntensity;
                }
                
                // Combine the texture alpha with the tint alpha for transparency.
                half finalAlpha = baseTex.a * _BaseColor.a;
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }

        //-------------------------------------------------------------------------------------
        // Pass 2: Outline Pass
        // This pass draws an outline by extruding the model and rendering only its backfaces.
        //-------------------------------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Cull Front // The magic trick: render the back of the polygons, not the front.
            
            // Outlines should not be transparent
            Blend Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            // Outline Vertex Shader
            Varyings vert(Attributes v)
            {
                Varyings o;
                // Take the vertex position and push it outwards along its normal.
                float4 position = v.positionOS;
                position.xyz += normalize(v.normalOS) * _OutlineWidth;

                // Transform the extruded vertex to screen space.
                o.positionHCS = TransformObjectToHClip(position.xyz);
                return o;
            }

            // Outline Fragment Shader
            half4 frag(Varyings i) : SV_Target
            {
                // Simply return the outline color.
                return _OutlineColor;
            }
            ENDHLSL
        }

        //-------------------------------------------------------------------------------------
        // Pass 3: Shadow Caster Pass (Corrected for Unity 6 / Modern URP)
        //-------------------------------------------------------------------------------------

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // These includes are required for the manual shadow casting pass to work correctly.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;

                // Manually perform the shadow caster transformation to avoid issues with GetShadowPositionHClip.
                // 1. Transform vertex position and normal from object to world space.
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                // 2. Apply shadow biasing to prevent self-shadowing artifacts.
                // _MainLightPosition.xyz provides the light direction for directional lights.
                float3 biasedPositionWS = ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz);

                // 3. Transform the biased world position to the light's clip space.
                // In a ShadowCaster pass, the view-projection matrix is already set up for the light.
                output.positionCS = TransformWorldToHClip(biasedPositionWS);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                // Shadow caster fragment shader doesn't need to output a color.
                return 0;
            }
            ENDHLSL
        }
    }
}

