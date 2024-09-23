Shader "Custom/DeltaVertexShader_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UVOffset ("UV Offset", Vector) = (0, 0, 1, 1)

        // uv map for the wireframe: nrtools/gpuskinning/tankuv.png
        //        _UV ("Wireframe UV Map", 2D) = "white" {}
        // served as glow color for the wireframe
        //        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                uint id : SV_VertexID;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                // wireframe uv/original
                float2 wireframe_uv : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_INSTANCING_BUFFER_START(Props)

            // frame indexing
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameOffset)

            // can we use this to interpolate between animations when blend factor is not 0?
            UNITY_DEFINE_INSTANCED_PROP(int, _NextAnimationOffset)
            UNITY_DEFINE_INSTANCED_PROP(float, _BlendFactor)

            UNITY_DEFINE_INSTANCED_PROP(float, _InterpolationFactor)

            // vertex index wrapping
            UNITY_DEFINE_INSTANCED_PROP(int, _VertexCount)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

            // scaling and offset
            UNITY_DEFINE_INSTANCED_PROP(float, _LocalScale)
            UNITY_DEFINE_INSTANCED_PROP(float3, _PivotOffset)

            // uv map for atlas
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVOffset)

            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            StructuredBuffer<float3> vertices;

            v2f vert(const appdata v, uint id : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                const int v_count = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexCount);
                if (v_count == 0)
                {
                    o.position = TransformObjectToHClip(v.vertex);
                    float4 uv_offset = UNITY_ACCESS_INSTANCED_PROP(Props, _UVOffset);
                    o.wireframe_uv = v.uv;
                    o.uv = v.uv * uv_offset.zw + uv_offset.xy;
                    return o;
                }
                const int frame_offset = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameOffset);
                const float interpolation = clamp(UNITY_ACCESS_INSTANCED_PROP(Props, _InterpolationFactor), 0.0, 1.0);

                const int delta_index0 = frame_offset + id;
                const int delta_index1 = frame_offset + id + v_count;

                float3 interpolated_delta =
                    lerp(vertices[delta_index0].xyz,
                    vertices[delta_index1].xyz, interpolation);
                
                const float anim_interp = UNITY_ACCESS_INSTANCED_PROP(Props, _BlendFactor);
                if (anim_interp > 0.0)
                {
                    const int next_anim_offset = UNITY_ACCESS_INSTANCED_PROP(Props, _NextAnimationOffset);
                    const float3 next_anim_vertex = vertices[next_anim_offset + id];
                    interpolated_delta = lerp(interpolated_delta, next_anim_vertex, anim_interp);
                }
                
                o.position = TransformObjectToHClip(interpolated_delta);
                o.wireframe_uv = v.uv;
                float4 uv_offset = UNITY_ACCESS_INSTANCED_PROP(Props, _UVOffset);
                o.uv = v.uv * uv_offset.zw + uv_offset.xy;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return texColor;
                // code for wireframe
                // half4 uvColor = SAMPLE_TEXTURE2D(_UV, sampler_UV, i.wireframe_uv);
                // const float3 glowColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                //
                // if (uvColor.r < 0.15)
                // {
                //     return float4(lerp(float3(0, 0, 0), texColor, 0), 1);
                // }
                //
                // const float wireframeIntensity = 1;
                // float3 wireframeGlow = lerp(float3(0, 0, 0), glowColor, wireframeIntensity);
                // return float4(wireframeGlow, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}