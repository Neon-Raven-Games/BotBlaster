Shader "NR/BlendShapeAnimation_URP"
{
    Properties
    {
        _BlendShapeTextureArray("Blend Shape Texture Array", 2DArray) = "" {}
        _UVOffset("UV Offset", Vector) = (0, 0, 1, 1)
        _AtlasIndex("Atlas Index", Float) = 0
        _FrameIndex("Frame Index", Float) = 0
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
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag

            // Enable instancing
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 blendUV : TEXCOORD1; // Custom UV or blend shape UV
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 blendUV : TEXCOORD1; // Pass blend UV
            };

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _UVOffset)
            UNITY_DEFINE_INSTANCED_PROP(float, _AtlasIndex)
            UNITY_DEFINE_INSTANCED_PROP(float, _FrameIndex)
            UNITY_INSTANCING_BUFFER_END(Props)

            TEXTURE2D_ARRAY(_BlendShapeTextureArray);
            SAMPLER(sampler_BlendShapeTextureArray);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 objectPosition = IN.positionOS;

                // Get the current frame index (sample the blend shape texture array)
                float frameIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameIndex);

                // Sample the blend shape delta for the current vertex from the texture array
                half4 blendShapeDelta = _BlendShapeTextureArray.SampleLevel(
                    sampler_BlendShapeTextureArray,
                    float3(IN.blendUV, frameIndex), 0 // Use blendUV and frameIndex for sampling
                );
                
                float blendShapeScale = .1; // Experiment with different values
                objectPosition.xyz += blendShapeDelta.xyz * blendShapeScale;
                // Apply the blend shape delta to the vertex position

                OUT.positionHCS = TransformObjectToHClip(objectPosition);

                // Pass UVs
                float4 uvOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _UVOffset);
                OUT.uv = IN.uv * uvOffset.zw + uvOffset.xy;
                OUT.blendUV = IN.blendUV;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Visualize the blend shape delta as a color for debugging
                float atlasIndex = UNITY_ACCESS_INSTANCED_PROP(Props, _FrameIndex);
                half4 blendShapeDelta = _BlendShapeTextureArray.SampleLevel(
                    sampler_BlendShapeTextureArray, float3(IN.blendUV, atlasIndex), 0);

                // Output the blend shape delta to visualize
                return half4(blendShapeDelta.rgb, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}