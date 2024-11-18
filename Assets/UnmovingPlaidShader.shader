Shader "Custom/ScreenSpaceTextureWithMask"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}  // Main texture that will cover the screen
        _MaskTex ("Mask Texture", 2D) = "white" {}   // Shape mask texture
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
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
                float4 pos : SV_POSITION;
                float2 screenUV : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate screen-space UVs
                o.screenUV = o.pos.xy * 0.5 + 0.5;

                // Use original UV for the mask
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture using screen-space UVs
                fixed4 mainColor = tex2D(_MainTex, i.screenUV);

                // Sample the mask texture using the original UVs to get the shape's alpha
                fixed4 maskColor = tex2D(_MaskTex, i.uv);

                // Apply the mask's alpha to the main texture color
                mainColor.a *= maskColor.a;

                return mainColor;
            }
            ENDCG
        }
    }
}
