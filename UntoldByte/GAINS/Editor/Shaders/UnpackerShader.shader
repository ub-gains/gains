Shader "Hidden/UntoldByte/GAINS/UnpackerShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile GAINS_GL GAINS_DX GAINS_VK

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            int scale;
            int2 texturePosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                int xIndex = texturePosition.x;
                #if defined(GAINS_GL)
                int yIndex = texturePosition.y; 
                #elif defined(GAINS_DX)
                int yIndex = scale - texturePosition.y - 1;
                #elif defined(GAINS_VK)
                int yIndex = texturePosition.y;
                //int yIndex = (scale - 1) - texturePosition.y;
                #endif

                float2 shift = float2(xIndex, yIndex);

                #if defined(GAINS_VK)
                shift.y = (scale - 1) - shift.y;
                #endif

                o.uv = v.uv / scale + shift / scale;

                return o;
            }

            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
