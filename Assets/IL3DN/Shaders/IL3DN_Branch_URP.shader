// Converted to Universal Render Pipeline
// Original: IL3DN/Branch (Amplify Shader Editor)
Shader "IL3DN/URP/Branch"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        [NoScaleOffset] NoiseTextureFloat("NoiseTexture", 2D) = "white" {}
        [Toggle(_WIND_ON)] _Wind("Wind", Float) = 1
        _WindStrenght("Wind Strenght", Range(0, 1)) = 0.5
        
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        LOD 300
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma shader_feature_local _WIND_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(NoiseTextureFloat);
            SAMPLER(samplerNoiseTextureFloat);

            // Global wind parameters (should be set via script)
            float3 WindDirection;
            float WindPower;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _WindStrenght;
            CBUFFER_END

            // Wind animation function
            float3 ApplyWind(float3 positionWS, float3 positionOS, float vertexColor)
            {
                #ifdef _WIND_ON
                    // Use world position for noise sampling
                    float2 noiseUV = positionWS.xz * 0.1;
                    
                    // Animated noise sampling
                    float time = _Time.y * 0.5;
                    float noise = SAMPLE_TEXTURE2D_LOD(NoiseTextureFloat, samplerNoiseTextureFloat, noiseUV + float2(time, time * 0.5), 0).r;
                    
                    // Wind direction (default to X if not set)
                    float3 windDir = length(WindDirection) > 0.01 ? normalize(WindDirection) : float3(1, 0, 0);
                    float windStr = max(0.01, WindPower) * _WindStrenght;
                    
                    // Apply wind based on vertex color (red channel = wind influence)
                    float windInfluence = vertexColor * noise;
                    float3 windOffset = windDir * windInfluence * windStr;
                    
                    return positionWS + windOffset;
                #else
                    return positionWS;
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                // Apply wind animation
                float3 positionWS = ApplyWind(vertexInput.positionWS, input.positionOS.xyz, input.color.r);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Sample texture and apply color
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 albedo = saturate(_Color * texColor);

                // Lighting setup
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalize(input.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lightingInput.fogCoord = input.fogFactor;
                lightingInput.bakedGI = SAMPLE_GI(0, 0, input.normalWS);
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lightingInput.shadowMask = SAMPLE_SHADOWMASK(0);

                // Lambert shading (simpler than PBR, matches original)
                half4 color = half4(0, 0, 0, 1);
                Light mainLight = GetMainLight(lightingInput.shadowCoord);
                
                half NdotL = saturate(dot(lightingInput.normalWS, mainLight.direction));
                half3 lighting = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * NdotL;
                
                // Ambient
                half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
                
                color.rgb = albedo.rgb * (lighting + ambient);
                color.rgb = MixFog(color.rgb, lightingInput.fogCoord);

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _WIND_ON
            #pragma multi_compile_instancing
            
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(NoiseTextureFloat);
            SAMPLER(samplerNoiseTextureFloat);

            float3 WindDirection;
            float WindPower;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _WindStrenght;
            CBUFFER_END

            float3 _LightDirection;

            float3 ApplyWind(float3 positionWS, float3 positionOS, float vertexColor)
            {
                #ifdef _WIND_ON
                    float2 noiseUV = positionWS.xz * 0.1;
                    float time = _Time.y * 0.5;
                    float noise = SAMPLE_TEXTURE2D_LOD(NoiseTextureFloat, samplerNoiseTextureFloat, noiseUV + float2(time, time * 0.5), 0).r;
                    
                    float3 windDir = length(WindDirection) > 0.01 ? normalize(WindDirection) : float3(1, 0, 0);
                    float windStr = max(0.01, WindPower) * _WindStrenght;
                    
                    float windInfluence = vertexColor * noise;
                    float3 windOffset = windDir * windInfluence * windStr;
                    
                    return positionWS + windOffset;
                #else
                    return positionWS;
                #endif
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                float3 positionWS = ApplyWind(vertexInput.positionWS, input.positionOS.xyz, input.color.r);
                
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _WIND_ON
            #pragma multi_compile_instancing
            
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(NoiseTextureFloat);
            SAMPLER(samplerNoiseTextureFloat);

            float3 WindDirection;
            float WindPower;

            CBUFFER_START(UnityPerMaterial)
                float _WindStrenght;
            CBUFFER_END

            float3 ApplyWind(float3 positionWS, float3 positionOS, float vertexColor)
            {
                #ifdef _WIND_ON
                    float2 noiseUV = positionWS.xz * 0.1;
                    float time = _Time.y * 0.5;
                    float noise = SAMPLE_TEXTURE2D_LOD(NoiseTextureFloat, samplerNoiseTextureFloat, noiseUV + float2(time, time * 0.5), 0).r;
                    
                    float3 windDir = length(WindDirection) > 0.01 ? normalize(WindDirection) : float3(1, 0, 0);
                    float windStr = max(0.01, WindPower) * _WindStrenght;
                    
                    float windInfluence = vertexColor * noise;
                    float3 windOffset = windDir * windInfluence * windStr;
                    
                    return positionWS + windOffset;
                #else
                    return positionWS;
                #endif
            }

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                float3 positionWS = ApplyWind(vertexInput.positionWS, input.positionOS.xyz, input.color.r);

                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
