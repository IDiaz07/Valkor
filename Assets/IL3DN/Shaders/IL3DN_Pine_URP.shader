// Converted to Universal Render Pipeline
// Original: IL3DN/Pine (Amplify Shader Editor)
Shader "IL3DN/URP/Pine"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _AlphaCutoff("Alpha Cutoff", Range(0, 1)) = 0.5
        _MainTex("MainTex", 2D) = "white" {}
        [NoScaleOffset] NoiseTextureFloat("NoiseTexture", 2D) = "white" {}
        [Toggle(_WIND_ON)] _Wind("Wind", Float) = 1
        _WindStrenght("Wind Strenght", Range(0, 1)) = 0.5
        [Toggle(_WIGGLE_ON)] _Wiggle("Wiggle", Float) = 1
        _WiggleStrenght("Wiggle Strenght", Range(0, 1)) = 0.5
        
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 1.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 0.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "TransparentCutout"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "AlphaTest"
        }
        
        LOD 300
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            
            #pragma shader_feature_local _WIND_ON
            #pragma shader_feature_local _WIGGLE_ON
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

            float3 WindDirection;
            float WindPower;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _AlphaCutoff;
                float _WindStrenght;
                float _WiggleStrenght;
            CBUFFER_END

            // Rotation matrix for wiggle effect
            float2 RotateUV(float2 uv, float2 pivot, float rotation)
            {
                float cosAngle = cos(rotation);
                float sinAngle = sin(rotation);
                
                uv -= pivot;
                float2 rotated;
                rotated.x = uv.x * cosAngle - uv.y * sinAngle;
                rotated.y = uv.x * sinAngle + uv.y * cosAngle;
                rotated += pivot;
                
                return rotated;
            }

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

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                float3 positionWS = ApplyWind(vertexInput.positionWS, input.positionOS.xyz, input.color.r);

                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = normalInput.normalWS;
                
                // Apply wiggle to UVs
                float2 baseUV = TRANSFORM_TEX(input.uv, _MainTex);
                
                #ifdef _WIGGLE_ON
                    float wiggleTime = _Time.y * 2.0;
                    float wiggleNoise = SAMPLE_TEXTURE2D_LOD(NoiseTextureFloat, samplerNoiseTextureFloat, 
                                                             positionWS.xz * 0.05 + wiggleTime * 0.1, 0).r;
                    float wiggleAngle = (wiggleNoise - 0.5) * _WiggleStrenght * input.color.r;
                    output.uv = RotateUV(baseUV, float2(0.5, 0.5), wiggleAngle);
                #else
                    output.uv = baseUV;
                #endif
                
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 albedo = _Color * texColor;
                
                // Alpha test
                clip(albedo.a - _AlphaCutoff);

                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalize(input.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                lightingInput.fogCoord = input.fogFactor;
                lightingInput.bakedGI = SAMPLE_GI(0, 0, input.normalWS);
                lightingInput.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                lightingInput.shadowMask = SAMPLE_SHADOWMASK(0);

                // Lambert shading
                half4 color = half4(0, 0, 0, 1);
                Light mainLight = GetMainLight(lightingInput.shadowCoord);
                
                half NdotL = saturate(dot(lightingInput.normalWS, mainLight.direction));
                half3 lighting = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * NdotL;
                
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
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _WIND_ON
            #pragma shader_feature_local _WIGGLE_ON
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
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(NoiseTextureFloat);
            SAMPLER(samplerNoiseTextureFloat);

            float3 WindDirection;
            float WindPower;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _AlphaCutoff;
                float _WindStrenght;
                float _WiggleStrenght;
            CBUFFER_END

            float3 _LightDirection;

            float2 RotateUV(float2 uv, float2 pivot, float rotation)
            {
                float cosAngle = cos(rotation);
                float sinAngle = sin(rotation);
                uv -= pivot;
                float2 rotated;
                rotated.x = uv.x * cosAngle - uv.y * sinAngle;
                rotated.y = uv.x * sinAngle + uv.y * cosAngle;
                rotated += pivot;
                return rotated;
            }

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

                float2 baseUV = TRANSFORM_TEX(input.uv, _MainTex);
                
                #ifdef _WIGGLE_ON
                    float wiggleTime = _Time.y * 2.0;
                    float wiggleNoise = SAMPLE_TEXTURE2D_LOD(NoiseTextureFloat, samplerNoiseTextureFloat, 
                                                             positionWS.xz * 0.05 + wiggleTime * 0.1, 0).r;
                    float wiggleAngle = (wiggleNoise - 0.5) * _WiggleStrenght * input.color.r;
                    output.uv = RotateUV(baseUV, float2(0.5, 0.5), wiggleAngle);
                #else
                    output.uv = baseUV;
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = _Color.a * texColor.a;
                clip(alpha - _AlphaCutoff);
                
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
            Cull Off

            HLSLPROGRAM
            #pragma target 3.0
            #pragma shader_feature_local _WIND_ON
            #pragma shader_feature_local _WIGGLE_ON
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
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(NoiseTextureFloat);
            SAMPLER(samplerNoiseTextureFloat);

            float3 WindDirection;
            float WindPower;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _AlphaCutoff;
                float _WindStrenght;
                float _WiggleStrenght;
            CBUFFER_END

            float2 RotateUV(float2 uv, float2 pivot, float rotation)
            {
                float cosAngle = cos(rotation);
                float sinAngle = sin(rotation);
                uv -= pivot;
                float2 rotated;
                rotated.x = uv.x * cosAngle - uv.y * sinAngle;
                rotated.y = uv.x * sinAngle + uv.y * cosAngle;
                rotated += pivot;
                return rotated;
            }

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
                
                float2 baseUV = TRANSFORM_TEX(input.uv, _MainTex);
                
                #ifdef _WIGGLE_ON
                    float wiggleTime = _Time.y * 2.0;
                    float wiggleNoise = SAMPLE_TEXTURE2D_LOD(NoiseTextureFloat, samplerNoiseTextureFloat, 
                                                             positionWS.xz * 0.05 + wiggleTime * 0.1, 0).r;
                    float wiggleAngle = (wiggleNoise - 0.5) * _WiggleStrenght * input.color.r;
                    output.uv = RotateUV(baseUV, float2(0.5, 0.5), wiggleAngle);
                #else
                    output.uv = baseUV;
                #endif

                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = _Color.a * texColor.a;
                clip(alpha - _AlphaCutoff);
                
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
