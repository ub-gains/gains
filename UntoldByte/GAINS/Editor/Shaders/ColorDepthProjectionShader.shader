Shader "Hidden/UntoldByte/GAINS/ColorDepthProjectionShader"
{
    Properties
    {
        [PowerSlider(1.5)]
        _MinDepthDiff ("Minimal depth difference", Range(0.00005, 0.005)) = 0.0002

        [PowerSlider(2.0)]
        _DepthError ("Depth error", Range(0.0001, 2.0)) = 0.0025
        
        [PowerSlider(2.0)]
        _NormalCutoff ("Normal Cutoff", Range(0.01, 0.99)) = 0.25

        [PowerSlider(2.0)]
        _NormalPower ("Normal Power", Range(0.5, 10)) = 6

        [PowerSlider(2.0)]
        _FavourCloser ("Favour closer", Range(0.01, 0.99)) = 0.95

        [MaterialToggle] 
        _UVMesh ("Use UV Mesh", float) = 0

        [IntRange]
        _NumberOfProjections ("Number of projections", Range(1,9)) = 0
        
        [MainTexture]
        _ProjectionColor ("Color Projection Texture", 2DArray) = "white" {}
        _ProjectionDepth ("Depth Projection Texture", 2DArray) = "white" {}
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
            #pragma target 3.5
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };

            UNITY_DECLARE_TEX2DARRAY(_ProjectionColor);
            UNITY_DECLARE_TEX2DARRAY(_ProjectionDepth);

            float _MinDepthDiff;
            float _DepthError;
            float _NormalCutoff;
            float _NormalPower;
            float _FavourCloser;
            float _UVMesh;
            int _NumberOfProjections;

            v2f vert (appdata v)
            {
                v2f o;

                if(_UVMesh == 1)
                    v.vertex = float4(v.uv.xy, 0, 1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 finalColor = half4(0,0,0,0);
                float multiplier = 0;
                float4 uv = i.uv;


                half4 depth = UNITY_SAMPLE_TEX2DARRAY(_ProjectionDepth, float3(uv.xy, 0));
                half minDepth = depth.y;
                half maxDepth = depth.y;

                for(int index0 = 1; index0 < _NumberOfProjections; index0++)
                {
                    half4 depth = UNITY_SAMPLE_TEX2DARRAY(_ProjectionDepth, float3(uv.xy, index0));
                    minDepth = min(minDepth, depth.y);
                    maxDepth = max(maxDepth, depth.y);
                }

                for(int index1 = 0; index1 < _NumberOfProjections; index1++)
                {
                    half4 col = UNITY_SAMPLE_TEX2DARRAY(_ProjectionColor, float3(uv.xy, index1));
                    half4 depth = UNITY_SAMPLE_TEX2DARRAY(_ProjectionDepth, float3(uv.xy, index1));

                    float minDepthDifference = depth.x;
                    float projectionAdjustedGeometryDepth = depth.y;
                    if(minDepthDifference < _MinDepthDiff)
                        minDepthDifference = _MinDepthDiff;

                    float factor = 1 + (maxDepth == minDepth ? 0 : _FavourCloser * 2 * (0.5 - (depth.y - minDepth)/(maxDepth - minDepth)));

                    if(any(col != half4(0,0,0,0)) && minDepthDifference <= _DepthError)
                    {
                        float strength = pow(clamp(depth.z - _NormalCutoff, 0, 1 - _NormalCutoff), _NormalPower) * 100 * factor * _DepthError / minDepthDifference;
                        multiplier += strength;
                        finalColor += col * strength; 
                    }
                }

                if(multiplier == 0) discard;

                finalColor /= multiplier;
                finalColor.w = 1;

                //// apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return finalColor;
            }

            ENDCG
        }
    }
}
