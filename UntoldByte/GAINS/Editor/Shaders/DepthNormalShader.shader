Shader "Hidden/UntoldByte/GAINS/DepthNormalShader"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off 
        ZWrite On 
        ZTest LEqual

        Pass
        {
            CGPROGRAM
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
            };

            float4 _MyProjectionParams;
            float4 _MyZBufferParams;

            float MyLinearEyeDepth(float depth)
            {
                return 1.0 / (_MyZBufferParams.z * depth + _MyZBufferParams.w);
            }

            float MyPreciseDepth(float depth)
            {
                float perspective = MyLinearEyeDepth(depth);
                float orthographic = (_MyProjectionParams.z - _MyProjectionParams.y) * (1 - depth) + _MyProjectionParams.y;
                return lerp(perspective, orthographic, unity_OrthoParams.w);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                half3 wNormal = UnityObjectToWorldNormal(v.normal);
                o.normal = mul((float3x3)UNITY_MATRIX_V, wNormal);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float depth = i.vertex.z;
                #if defined(UNITY_REVERSED_Z)
                    
                #else
                     depth = 1.0f - depth;
                #endif
                depth = MyPreciseDepth(depth);
                return float4(depth, i.normal);
            }
            ENDCG
        }
    }
}
