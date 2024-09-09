Shader "Custom/AtlasShader_URP_VR"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // Atlas texture
        _UVOffset ("UV Offset", Vector) = (0, 0, 1, 1)  // UV Offset (x, y, width, height)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Enable instancing support
            #pragma multi_compile_instancing

            // Include URP Core for lighting and VR stereo macros
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // Define the structure of input attributes
            struct Attributes
            {
                float4 positionOS : POSITION;  // Object space vertex position
                float2 uv : TEXCOORD0;  // UV coordinates

                UNITY_VERTEX_INPUT_INSTANCE_ID  // Instancing ID for GPU instancing
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;  // Homogeneous clip space position
                float2 uv : TEXCOORD0;  // UV coordinates

                UNITY_VERTEX_INPUT_INSTANCE_ID  // Instance ID for GPU instancing
                UNITY_VERTEX_OUTPUT_STEREO  // Stereo output for VR rendering
            };

            // Instanced property for UV offset
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _UVOffset)  // Per-instance UV offset
            UNITY_INSTANCING_BUFFER_END(Props)

            // Declare texture and sampler
            TEXTURE2D(_MainTex);  // Atlas texture
            SAMPLER(sampler_MainTex);

            // Vertex Shader
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                UNITY_SETUP_INSTANCE_ID(IN);  // Set up instance ID for instancing
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);  // Set up stereo rendering for VR

                // Transform vertex to clip space (HCS = Homogeneous Clip Space)
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);  // Using URP's transform macro

                // Access UV offset from the instancing property buffer
                float4 uvOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _UVOffset);

                // Apply UV offset and scale to the input UV coordinates
                OUT.uv = IN.uv * uvOffset.zw + uvOffset.xy;

                return OUT;
            }

            // Fragment Shader
            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);  // Set up stereo eye index for VR rendering

                // Sample the texture using the modified UV coordinates
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                return color;
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
