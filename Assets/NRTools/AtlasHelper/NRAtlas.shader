Shader "Custom/AtlasShader_URP"
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
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _UVOffset;  // UV Rect offset (x, y) and scale (width, height)
                TEXTURE2D(_MainTex); // Atlas texture
                SAMPLER(sampler_MainTex);  // Sampler for the atlas texture
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);

                // Apply UV offset and scale
                OUT.uv = IN.uv * _UVOffset.zw + _UVOffset.xy;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample the main texture using the modified UV coordinates
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
