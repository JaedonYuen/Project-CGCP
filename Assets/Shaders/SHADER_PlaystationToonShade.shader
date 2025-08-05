Shader "Custom/PlaystationCelshader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RoundTo ("Round To", Float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        //_Shine ("Shine", Float) = 0.5
        _Specularity ("Specularity", Float) = 0.5
        _ShadingSteps ("Toon Steps", Range(2, 10)) = 3
        _ShadingSmoothness ("Toon Smoothness", Range(0.001, 0.1)) = 0.01
        _OutlineWidth ("Outline Width", Float) = 0.05
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                float3 worldNormal : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                fixed4 rimColor : TEXCOORD4; // For outline shading
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RoundTo;
            float4 _Color;
            //float _Shine;
            float _Specularity;
            float _ShadingSteps;
            float _ShadingSmoothness;
            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;

                // Transform to camera space
                float3 camPos = mul(UNITY_MATRIX_MV, v.vertex).xyz;

                // Snap in camera space for that PSX effect
                camPos = round(camPos / _RoundTo) * _RoundTo;

                // Transform back to clip space
                float4 snappedCamPos = float4(camPos, 1.0);
                o.vertex = mul(UNITY_MATRIX_P, snappedCamPos);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);

                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldNormal = worldNormal;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                

                /// Calculate outline lighting
                fixed3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                fixed dotProduct = 1-saturate(dot(v.normal, viewDir));
                o.rimColor = lerp(1-_OutlineWidth, 1.0, dotProduct);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                //Diffuse
                float nl = max(0, dot(i.worldNormal, _WorldSpaceLightPos0.xyz));

                //step
                float toonDiffuse = floor(nl* _ShadingSteps) / _ShadingSteps;

                //Apply
                fixed4 diffuseColor = toonDiffuse * _LightColor0;
                diffuseColor.rgb += ShadeSH9(half4(normalize(i.worldNormal), 1));
                col *= diffuseColor;

                //Specular
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 reflectDir = reflect(-lightDir, normalize(i.worldNormal));

                float spec = max(0, dot(viewDir, reflectDir));
                spec = pow(spec, _Specularity * 128.0); // Adjust specularity
                spec = step(0.5, spec); // Toon specular steps
                col.rgb += spec * _LightColor0.rgb;

                // Outline
                fixed fresnelFactor = saturate(lerp(0.0, 1.0, i.rimColor.r));
                fresnelFactor = step(0.5, fresnelFactor); // make it just a thick flat outline
                col.a = lerp(col.a,1.0,fresnelFactor);
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, fresnelFactor);
                
                col *= _Color;
                //fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
