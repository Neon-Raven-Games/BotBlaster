Shader "Custom/DualQuat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Atlas texture
        _UVOffset ("UV Offset", Vector) = (0, 0, 1, 1) // UV Offset (x, y, width, height)
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
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

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
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameCount)
            UNITY_DEFINE_INSTANCED_PROP(float, _InterpolationFactor)
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

            StructuredBuffer<vertex_info> _Vertices;
            StructuredBuffer<vertex_info> _BlendResult;

            v2f vert(const appdata v, uint id : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 modified_vertex = lerp(_Vertices[id].position.xyz, _BlendResult[id].position.xyz, 0.5);

                o.position = TransformObjectToHClip(modified_vertex.xyz);
                o.uv = v.uv;

                const float3 modified_normal = normalize(lerp(_Vertices[id].normal, _BlendResult[id].normal, 0.5)); // Example
                o.normal = modified_normal;

                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                const int frame_index = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameIndex);
                const int frame_count = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameCount);

                // Normalize the frame index between 0 and 1
                const float normalized_frame = float(frame_index) / float(frame_count - 1);

                // Define a simple color gradient from red to blue
                const float3 start_color = float3(1, 0, 0); // Red
                const float3 end_color = float3(0, 0, 1); // Blue
                float3 interpolated_color = lerp(start_color, end_color, normalized_frame);
                return float4(interpolated_color, 1.0); // This is not used

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}