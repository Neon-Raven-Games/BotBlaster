Shader "Custom/DeltaVertexShader_URP"
{
    Properties
    {
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

            // Include URP-specific shader libraries
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Structs for passing data between vertex and fragment shaders
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID // Instancing ID for GPU instancing
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID // Instance ID for GPU instancing
                UNITY_VERTEX_OUTPUT_STEREO // Stereo output for VR rendering
            };

            // Declare buffers outside instancing system
            StructuredBuffer<int> _VertexIDs;
            StructuredBuffer<float3> _Deltas;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _FrameIndex) // Current frame index for each instance
                UNITY_DEFINE_INSTANCED_PROP(float, _InterpolationFactor) // Interpolation factor between frames
            UNITY_INSTANCING_BUFFER_END(Props)
            
            v2f vert(appdata v, uint id : SV_VertexID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);  // Set up instance ID for instancing
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);  // Set up stereo rendering for VR

                // Get the vertex ID
                int vertexID = _VertexIDs[id];

                // Get instanced properties: frame index and interpolation factor
                float frameIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameIndex);
                float interpolation = UNITY_ACCESS_INSTANCED_PROP(Props, _InterpolationFactor);

                // Calculate the delta offset based on frame index
                // Assuming the delta buffer stores frames sequentially: deltas for each vertex in frame 0, then frame 1, and so on.
                int frameCount = 5; // The total number of frames (should match the total frame count)
                int deltaIndex0 = vertexID + int(frameIndex) * frameCount;
                int deltaIndex1 = vertexID + (int(frameIndex) + 1) * frameCount;

                // Fetch the deltas for frame 0 and frame 1
                float3 delta0 = _Deltas[deltaIndex0];
                float3 delta1 = _Deltas[deltaIndex1];

                // Interpolate between frame 0 and frame 1
                float3 interpolatedDelta = lerp(delta0, delta1, interpolation);

                // Apply the delta to the vertex position
                float4 modifiedVertex = v.vertex;
                modifiedVertex.xyz += interpolatedDelta;

                // Output the transformed position
                o.position = TransformObjectToHClip(modifiedVertex.xyz);
                o.uv = v.uv;
                o.normal = v.normal;

                return o;
            }

             float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return float4(1, 1, 1, 1); 
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
