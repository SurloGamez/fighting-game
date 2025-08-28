Shader "Unlit/WhitenOpaquePixels"
{
    Properties
    {

        [PerRenderData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags { 
            //"RenderType"="Opaque" 
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            }
        //LOD 100
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                //float2 textcoord : TEXCOORD0;
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
            };

            sampler2D _MainTex;
            float _Cutoff
            //float4 _MainTex_ST;

            v2f vert (appdata IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // sample the texture
                fixed4 c = tex2D(_MainTex, IN.uv);
                //if pixel is not transparent, make it white
                if(c.a > _Cutoff)
                {
                    return fixed4(1,1,1,c.a);
                }
                else
                {
                    return fixed4(0,0,0,0);
                }
            }
            ENDCG
        }
    }
}
