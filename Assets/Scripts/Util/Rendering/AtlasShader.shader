Shader "NR/AtlasUVShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UVOffset ("UV Offset", Vector) = (0,0,1,1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _UVOffset;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = lerp(_UVOffset.xy, _UVOffset.zw, v.uv);  // Apply the UV offset
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);  // Sample the atlas with the adjusted UVs
            }
            ENDCG
        }
    }
}
