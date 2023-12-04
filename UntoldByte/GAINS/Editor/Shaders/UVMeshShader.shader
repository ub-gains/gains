Shader "Hidden/UntoldByte/GAINS/UVMeshShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            //// make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            
            float _UVMesh;

            v2f vert (appdata v)
            {
                v2f o;

                if(_UVMesh == 1)
                    v.vertex = float4(v.uv.xy, 0, 1);

                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = v.uv;

                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                //UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = float4(0.5, 0.5, 0.5, 1);//tex2D(_MainTex, i.uv);
                //// apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
