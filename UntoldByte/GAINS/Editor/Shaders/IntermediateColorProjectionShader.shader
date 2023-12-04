Shader "Hidden/UntoldByte/GAINS/IntermediateColorProjectionShader"
{
    Properties
    {
        [MaterialToggle] 
        _UVMesh ("Use UV Mesh", float) = 0

        [MainTexture]
        _ProjectionColor ("Projection Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            //// make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            UNITY_DECLARE_TEX2D(_ProjectionColor);

            float _UVMesh;
            int _UV;

            float4 TransformProjectionUV(float4 uv)
            {
                float2 zw = float2(1 / uv.z, uv.w == 0.0f ? -uv.z : uv.z);
                return float4(float2(uv.x, uv.y), zw);
            }

            v2f vert (appdata v)
            {
                v2f o;

                if(_UVMesh == 1)
                    v.vertex = float4(v.uv0.xy, 0, 1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TransformProjectionUV(v.uv1);

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 finalColor = half4(0,0,0,0);
                
                float4 uv = i.uv;

                if(uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) discard;

                half4 col = UNITY_SAMPLE_TEX2D(_ProjectionColor, uv.xy);

                float projectionAdjustedGeometryDepth = 1 / uv.z;
                float facing = sign(uv.w);

                if(facing >= 0.9f)
                {
                    finalColor = col;
                }

                return finalColor;
            }

            ENDCG
        }
    }
}
