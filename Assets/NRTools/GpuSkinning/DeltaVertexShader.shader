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
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Include URP-specific shader libraries
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Structs for passing data between vertex and fragment shaders
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            // Buffers for vertex IDs and deltas
            StructuredBuffer<int> _VertexIDs;
            StructuredBuffer<float3> _Deltas;

            v2f vert(appdata v, uint id : SV_VertexID)
            {
                v2f o;

                // Get the vertex ID
                int vertexID = _VertexIDs[id];

                // Fetch the delta for this vertex
                float3 delta = _Deltas[vertexID];

                // Apply the delta to the vertex position
                float4 modifiedVertex = v.vertex;
                modifiedVertex.xyz += delta;

                // Output the transformed position
                o.position = TransformObjectToHClip(modifiedVertex.xyz);
                o.uv = v.uv;
                o.normal = v.normal;

                return o;
            }
            
            // Simple fragment shader to output the base color
            float4 frag(v2f i) : SV_Target
            {
                return float4(1, 1, 1, 1); // Output white color
            }

            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
