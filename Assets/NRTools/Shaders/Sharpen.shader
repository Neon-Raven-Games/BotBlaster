Shader "Custom/sharpen"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Size ("Size", Range(0.00005, 0.0008)) = 0.0001
        _Inten ("Intensity", Range(0.5, 4)) = 2
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "LightMode" = "ForwardBase"
            }
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _Size;
            float _Inten;

            TEXTURE2D(_MainTex);           // Declare the texture
            SAMPLER(sampler_MainTex);      // Declare the sampler

            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = TransformObjectToHClip(v.vertex);   // Vertex to clip space position
                o.uv_MainTex = v.uv;                // Pass the texture coordinates
                return o;
            }
            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Sample the center pixel
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex);

                // Sample pixels in four directions with offsets
                float4 sampleLeft = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex + float2(-_Size, 0));
                float4 sampleRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex + float2(_Size, 0));
                float4 sampleUp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex + float2(0, _Size));
                float4 sampleDown = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex + float2(0, -_Size));

                // Apply sharpening effect (increase contrast between neighbors)
                float4 sharpened = color * (1 + 4.0 * _Inten) - (sampleLeft + sampleRight + sampleUp + sampleDown) * _Inten;

                return sharpened;  // Return the final color
            }
            ENDHLSL
        }
    }
}
