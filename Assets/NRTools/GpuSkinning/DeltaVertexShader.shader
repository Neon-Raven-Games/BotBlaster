Shader "Custom/DeltaVertexShader_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Atlas texture
        _UVOffset ("UV Offset", Vector) = (0, 0, 1, 1) // UV Offset (x, y, width, height)
        _FrameIndex ("Frame Index", Int) = 0
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "DualQuaternion/DualQuat.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint id : SV_VertexID;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameIndex)
            UNITY_DEFINE_INSTANCED_PROP(float, _InterpolationFactor)
            UNITY_DEFINE_INSTANCED_PROP(int, _VertexCount)
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameCount)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVOffset)
            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct vertex_info
            {
                float4 position;
                float4 normal;
                float4 tangent;

                int4 bone_indexes;
                float4 bone_weights;

                float compensation_coef;
            };

            struct morph_delta
            {
                float4 position;
                float4 normal;
                float4 tangent;
            };

            StructuredBuffer<vertex_info> vertices;
            StructuredBuffer<morph_delta> deltas;

            v2f vert(const appdata v, uint id : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                const int frame_index = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameIndex);
                const float interpolation = clamp(UNITY_ACCESS_INSTANCED_PROP(Props, _InterpolationFactor), 0.0, 1.0);
                const int vertex_count = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexCount);
                
                const int delta_index0 = frame_index * vertex_count + id;
                const int delta_index1 = (frame_index + 1) % UNITY_ACCESS_INSTANCED_PROP(Props, _FrameCount) *
                    vertex_count + id;


                const float3 interpolated_delta = lerp(deltas[delta_index0].position.xyz, deltas[delta_index1].position.xyz,
                                                       interpolation);

                float3 modified_vertex = deltas[delta_index1].position.xyz;
                modified_vertex.xyz += interpolated_delta.xyz;

                o.position = TransformObjectToHClip(modified_vertex);
                o.normal = TransformObjectToWorldNormal(v.normal);

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