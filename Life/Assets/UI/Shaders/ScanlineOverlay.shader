Shader "LifeLike/UI/ScanlineOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _ScanlineCount ("Scanline Count", Float) = 300
        _ScanlineSpeed ("Scanline Speed", Float) = 0.5
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.3
        _VignetteSmoothness ("Vignette Smoothness", Range(0.01, 1)) = 0.5
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.1)) = 0.02
        _NoiseIntensity ("Noise Intensity", Range(0, 0.2)) = 0.05
        _Brightness ("Brightness", Range(0.5, 1.5)) = 1.0
        _TintColor ("Tint Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineSpeed;
            float _VignetteIntensity;
            float _VignetteSmoothness;
            float _FlickerIntensity;
            float _NoiseIntensity;
            float _Brightness;
            float4 _TintColor;

            // ノイズ関数
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ベースカラー（完全透明から開始）
                fixed4 col = fixed4(0, 0, 0, 0);

                // スキャンライン効果
                float scanline = sin((i.uv.y * _ScanlineCount + _Time.y * _ScanlineSpeed) * 3.14159) * 0.5 + 0.5;
                scanline = pow(scanline, 2.0);
                float scanlineAlpha = scanline * _ScanlineIntensity;

                // ビネット効果
                float2 uvCentered = i.uv - 0.5;
                float vignette = 1.0 - dot(uvCentered, uvCentered) * 2.0;
                vignette = smoothstep(0.0, _VignetteSmoothness, vignette);
                float vignetteAlpha = (1.0 - vignette) * _VignetteIntensity;

                // フリッカー効果
                float flicker = random(float2(_Time.y * 10.0, 0.0)) * _FlickerIntensity;

                // ノイズ効果
                float noise = random(i.uv + _Time.y) * _NoiseIntensity;

                // 効果を合成（暗くする方向のオーバーレイ）
                float totalDarkening = scanlineAlpha + vignetteAlpha + flicker + noise;
                totalDarkening = saturate(totalDarkening);

                // 最終色（黒を重ねる）
                col = fixed4(0, 0, 0, totalDarkening * _TintColor.a);

                return col;
            }
            ENDCG
        }
    }
}
