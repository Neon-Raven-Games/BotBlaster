Shader "Custom/DeltaVertexShader_URP"
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

            StructuredBuffer<int> _VertexIDs;
            StructuredBuffer<float3> _Deltas;

            StructuredBuffer<float4x4> _BoneMatrices;
            StructuredBuffer<dual_quaternion> _BoneDQ;
            StructuredBuffer<int4> _BoneIndices;
            StructuredBuffer<float4> _BoneWeights;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameIndex)
            UNITY_DEFINE_INSTANCED_PROP(float, _InterpolationFactor)
            UNITY_DEFINE_INSTANCED_PROP(int, _VertexCount)
            UNITY_DEFINE_INSTANCED_PROP(int, _FrameCount)
            UNITY_DEFINE_INSTANCED_PROP(int, _BoneCount)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVOffset)
            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            v2f vert(const appdata v, uint id : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                const int frame_index = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameIndex);
                const float interpolation = UNITY_ACCESS_INSTANCED_PROP(Props, _InterpolationFactor);
                const int vertex_count = UNITY_ACCESS_INSTANCED_PROP(Props, _VertexCount);;
                const int delta_index0 = frame_index * vertex_count + _VertexIDs[id];
                const int delta_index1 = (frame_index + 1) * vertex_count + _VertexIDs[id];
                const float3 interpolated_delta = lerp(_Deltas[delta_index0], _Deltas[delta_index1], interpolation);

                float4 modified_vertex = v.vertex;
                modified_vertex.xyz += interpolated_delta;
                
                const int4 bone_indices = _BoneIndices[id];
                const float4 bone_weights = _BoneWeights[id];

                o.position = TransformObjectToHClip(modified_vertex.xyz);
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