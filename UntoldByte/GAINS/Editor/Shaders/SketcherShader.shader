Shader "Hidden/UntoldByte/GAINS/SketcherShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off 
        ZWrite Off 
        ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 _Position0;
            float2 _Position1;

            fixed4 _brushColor = fixed4(1,0,0,1);
            float _brushSize = 0.002f;

            sampler2D _MainTex;

            float PointDistance(float2 lp1, float2 lp2, float2 tp)
            {
                return (tp.x - lp1.x)*(lp2.y - lp1.y) - (tp.y - lp1.y)*(lp2.x - lp1.x);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv); 
                bool covered = length(i.uv - _Position0) <= _brushSize;
                covered = covered || length(i.uv - _Position1) <= _brushSize;

                float2 diff = _Position1 - _Position0;
                float A = diff.y;
                float B = -diff.x;
                float C = _Position0.y * diff.x - _Position0.x * diff.y;
                float distance = abs(A * i.uv.x + B * i.uv.y + C)/length(float2(A,B));

                float radius = length(float2(_brushSize,length(_Position1-_Position0)/2));
                float2 center = (_Position1 + _Position0)/2;
                covered = covered || (length(i.uv-center) <= radius && distance <= _brushSize);

                float shade = covered ? 1.0f : 0.0f;
                col = lerp(col, _brushColor, shade);
                return  col;
            }
            ENDCG
        }
    }
}