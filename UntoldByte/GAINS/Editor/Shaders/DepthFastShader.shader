Shader "Hidden/UntoldByte/GAINS/DepthFastShader"
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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float depth = i.vertex.z;
                #if defined(UNITY_REVERSED_Z)
                    
                #else
                    depth = 1.0f - depth;
                #endif

                return float4(depth, depth, depth, 1);
            }
            ENDCG
        }
    }
}
