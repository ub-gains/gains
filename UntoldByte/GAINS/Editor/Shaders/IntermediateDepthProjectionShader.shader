Shader "Hidden/UntoldByte/GAINS/IntermediateDepthProjectionShader"
{
    Properties
    {
        [MaterialToggle] 
        _UVMesh ("Use UV Mesh", float) = 0

        [MainTexture]
        _ProjectionLEDepth ("Projection Eye Depth Texture", 2D) = "" {}
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

            UNITY_DECLARE_TEX2D(_ProjectionLEDepth);
            float4 _ProjectionLEDepth_TexelSize;

            float _UVMesh;
            int _UV;

            static const float2 directions[4] = {float2(1,0), float2(0,1), float2(-1,0), float2(0,-1)}; 
            
            float2 MinDepthDifferenceWithNormal(float depth, float2 uv, float distance)
             {
                float4 sampleDepthNormal = UNITY_SAMPLE_TEX2D(_ProjectionLEDepth, uv);
                float sampleDepth = sampleDepthNormal.x;
                float sampleNormalZ = sampleDepthNormal.w;
                float difference = abs(depth - sampleDepth);
                float normalZ = sampleNormalZ;

                for(int i = 0; i < distance; i++)
                {
                    for(int j = 0; j < 4; j++)
                    {
                        float2 textureCoordinates = directions[j] * float2(_ProjectionLEDepth_TexelSize.x, _ProjectionLEDepth_TexelSize.y) * (i + 1);
                        sampleDepthNormal = UNITY_SAMPLE_TEX2D(_ProjectionLEDepth, uv + textureCoordinates);
                        sampleDepth = sampleDepthNormal.x;
                        difference = min(difference, abs(depth - sampleDepth));
                    }
                }

                return float2(difference, normalZ);
            }

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

            float4 frag (v2f i) : SV_Target
            {
                float4 uv = i.uv;

                if(uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) discard;

                float projectionAdjustedGeometryDepth = 1 / uv.z;

                float2 minDepthDifferenceNormal = MinDepthDifferenceWithNormal(projectionAdjustedGeometryDepth, uv.xy, 2);

                return float4(minDepthDifferenceNormal.x, projectionAdjustedGeometryDepth, minDepthDifferenceNormal.y, 1);
            }

            ENDCG
        }
    }
}
