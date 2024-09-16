Shader "Custom/DeltaVertexShader_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UVOffset ("UV Offset", Vector) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags{"RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

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

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_INSTANCING_BUFFER_START(Props)

            // frame indexing
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameOffset)
            UNITY_DEFINE_INSTANCED_PROP(float, _InterpolationFactor)

            // vertex index wrapping
            UNITY_DEFINE_INSTANCED_PROP(int, _VertexCount)

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
                    o.uv = v.uv * uv_offset.zw + uv_offset.xy;

                    return o;
                }
                const int frame_offset = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameOffset);
                const float interpolation = clamp(UNITY_ACCESS_INSTANCED_PROP(Props, _InterpolationFactor), 0.0, 1.0);

                const int delta_index0 = frame_offset + id;
                const int delta_index1 = frame_offset + id + v_count;

                float3 interpolated_delta = lerp(
                    vertices[delta_index0].xyz,
                    vertices[delta_index1].xyz,
                    interpolation);

                // const float local_scale = 1.0 / UNITY_ACCESS_INSTANCED_PROP(Props, _LocalScale);
                // interpolated_delta.xyz *= local_scale;

                o.position = TransformObjectToHClip(interpolated_delta);

                float4 uv_offset = UNITY_ACCESS_INSTANCED_PROP(Props, _UVOffset);
                o.uv = v.uv * uv_offset.zw + uv_offset.xy;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}