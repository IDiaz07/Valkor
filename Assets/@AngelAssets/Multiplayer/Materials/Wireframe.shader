Shader "Custom/URPCleanWireframe_AndroidVR"
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
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv2        : TEXCOORD2;
                float2 uv3        : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color      : COLOR; // Barycentric
                float3 vis        : TEXCOORD0; // Edge Visibility
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _WireColor;
                float _WireThickness;
            CBUFFER_END

            v2f vert(Attributes v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.color = v.color;
                o.vis = float3(v.uv2.x, v.uv2.y, v.uv3.x); // Combine our UVs into one vector
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 bary = i.color.rgb;
                float3 vis = i.vis;

                // If an edge is told to be invisible (vis < 0.5), push its barycentric value to 1.0 
                // so the smoothstep never draws it.
                float distX = (vis.x > 0.5) ? bary.x : 1.0;
                float distY = (vis.y > 0.5) ? bary.y : 1.0;
                float distZ = (vis.z > 0.5) ? bary.z : 1.0;

                float minBary = min(distX, min(distY, distZ));
                
                float delta = fwidth(minBary);
                float wire = smoothstep(_WireThickness, _WireThickness - delta, minBary);

                return float4(_WireColor.rgb, _WireColor.a * wire);
            }
            ENDHLSL
        }
    }
}