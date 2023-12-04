Shader "Hidden/UntoldByte/GAINS/DepthShader"
{
    Properties
    {
        [KeywordEnum(_0,_1)] _LinearEyeDepth("Linear eye depth",int) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _LinearEyeDepth__0 _LinearEyeDepth__1
            #pragma multi_compile _ GAINS_GL

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                half3 wNormal = UnityObjectToWorldNormal(v.normal);
                o.normal = mul((float3x3)UNITY_MATRIX_V, wNormal);
                o.uv = v.uv;
                return o;
            }

            sampler2D_float _CameraDepthTexture;

            float PreciseDepth(float depth)
            {
                float perspective = LinearEyeDepth(depth);
                float orthographic = (_ProjectionParams.z - _ProjectionParams.y) * (1 - depth) + _ProjectionParams.y;
                return lerp(perspective, orthographic, unity_OrthoParams.w);
            }

            float4 frag (v2f i) : SV_Target
            {
                 float depth = tex2D(_CameraDepthTexture, i.uv);
                 
                 #if defined(_LinearEyeDepth__1)
                 depth = PreciseDepth(depth);
                 #endif
                 
                 #if defined(GAINS_GL)
                    depth = 1.0f - depth;
                 #endif
                 
                 return float4(depth, i.normal.x, i.normal.y, i.normal.z);
            }
            ENDCG
        }
    }
}
