Shader "Custom/Sheild"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Float) = 0.5
        _Color ("Color", Color) = (1, 1, 1, 1)
        _FresnelWidth ("Fresnel Width", Range(0.0, 5.0)) = 1.5
        _WaveIntensity ("Wave Intensity", Range(0.0, 1.0)) = 0.5
        _OverlayBlend ("Overlay Blend", Range(0.0, 1.0)) = 0.5
        _ScanlineFrequency ("Scanline Frequency", Range(1.0, 1000.0)) = 20.0
        _ScanlineSpeed ("Scanline Speed", Range(0.0, 5.0)) = 1.0
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 1.0)) = 0.3
        _ScanlineSharpness ("Scanline Sharpness", Range(0.001, 10.0)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha 
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed4 diff : COLOR0;
                fixed4 rimColor : TEXCOORD1;
                float4 worldPos : TEXCOORD2;
                float4 screenPos : TEXCOORD3; // Add screen position for camera-relative scanlines
            };

            sampler2D _MainTex;
            sampler2D _OverlayTex; 
            float4 _MainTex_ST;
            float4 _OverlayTex_ST;
            float _ScrollSpeed;
            fixed4 _Color;
            float _FresnelWidth;
            float _WaveIntensity;
            float _OverlayBlend;
            float _ScanlineFrequency;
            float _ScanlineSpeed;
            float _ScanlineIntensity;
            float _ScanlineSharpness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Calculate world position
                o.screenPos = ComputeScreenPos(o.vertex); // Calculate screen position for scanlines
                _MainTex_ST.z = _Time * _ScrollSpeed;
                _OverlayTex_ST.z = 1-(_Time * _ScrollSpeed);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                
                // Calculate rim lighting
                fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                fixed dotProduct = 1-saturate(dot(v.normal, viewDir));
                o.rimColor = lerp(1-_FresnelWidth, 1.0, dotProduct);

                // Distort UVs for a cool wave

                o.uv.y += sin(_Time.y * _WaveIntensity + v.vertex.x * _WaveIntensity) ; // Wave effect
                o.uv.x += cos(_Time.y * _WaveIntensity + v.vertex.y * _WaveIntensity) ; // Wave effect
                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                col *= _Color;
                col.a = (col.r + col.g + col.b) / 3.0; // Average RGB for alpha
                
                fixed4 overlayCol = tex2D(_OverlayTex, i.uv);
                overlayCol *= _Color;
                overlayCol.a = (overlayCol.r + overlayCol.g + overlayCol.b) / 3.0; // Average RGB for alpha
                
                col = lerp(col, overlayCol, _OverlayBlend); //blend 

                // Apply scanline effect
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float scanline = sin((screenUV.y + _Time.y * _ScanlineSpeed) * _ScanlineFrequency);
                scanline = abs(scanline);
                scanline = pow(scanline, _ScanlineSharpness); // Make scanlines sharper

                // Apply fresnel effect

                fixed fresnelFactor = saturate(lerp(0.0, 1.0, i.rimColor.r));
                col.a = lerp(col.a,1.0,fresnelFactor);
                col.rgb = lerp(col.rgb, _Color, fresnelFactor);

                //Make it so only the image is affected by the scanlines
                scanline = lerp(1.0, scanline, _ScanlineIntensity);
                col.rgb *= scanline;
                col.a *= scanline;
                

                return col;
            }
            ENDCG
        }
    }
}
