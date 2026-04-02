Shader "Custom/URPWireframeTransparent"
{
    Properties
    {
        [HDR] _WireColor("Wire Color", Color) = (0, 1, 0, 1)
        _WireThickness("Wire Thickness", Range(0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ForwardLit"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off 

            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 projection : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 projection : SV_POSITION;
                float3 barycentric : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _WireColor;
                float _WireThickness;
            CBUFFER_END

            v2g vert(Attributes v)
            {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.projection = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }


            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;


                UNITY_SETUP_INSTANCE_ID(input[0]);

                float3 barys[3] = {
                    float3(1, 0, 0),
                    float3(0, 1, 0),
                    float3(0, 0, 1)
                };

                for (int i = 0; i < 3; i++)
                {

                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    UNITY_TRANSFER_INSTANCE_ID(input[i], o);
                    
                    o.projection = input[i].projection;
                    o.barycentric = barys[i];
                    triStream.Append(o);
                }
            }

            half4 frag(g2f i) : SV_Target
            {

                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float minBary = min(i.barycentric.x, min(i.barycentric.y, i.barycentric.z));
                float delta = fwidth(minBary);
                float wire = smoothstep(_WireThickness, _WireThickness - delta, minBary);

                return float4(_WireColor.rgb, _WireColor.a * wire);
            }
            ENDHLSL
        }
    }
}