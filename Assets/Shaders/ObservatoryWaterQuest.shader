Shader "Observatory/ObservatoryWaterQuest"
{
    Properties
    {
        [Header(Color and Transparency)]
        _ShallowWaterColor ("Shallow Water Color", Color) = (0.1, 0.4, 0.5, 0.85)
        _DeepWaterColor ("Deep Water Color", Color) = (0.05, 0.12, 0.2, 0.95)
        _Glossiness ("Smoothness", Range(0,1)) = 0.8
        _Metallic ("Metallic", Range(0,1)) = 0.3
        _WaveContrast ("Wave Shading Contrast", Range(0.5, 4)) = 1.5
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.4

        [Header(Waves)]
        _WaveHeight ("Wave Height", Range(0, 1.5)) = 0.25
        _WaveFrequency ("Wave Frequency", Float) = 0.8
        _WaveSpeed ("Wave Speed", Float) = 1.2
        _WaveDir ("Wave Direction (XY)", Vector) = (1, 0.3, 0, 0)

        [Header(Shoreline Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount ("Foam Amount", Range(0, 2)) = 0.6
        _FoamSpeed ("Foam Movement Speed", Float) = 0.8
        _FoamScale ("Foam Scale", Float) = 4.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
                float2 foamUV : TEXCOORD3;   // Added for foam
                UNITY_FOG_COORDS(4)
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _ShallowWaterColor;
            half4 _DeepWaterColor;
            half4 _FoamColor;
            half _Glossiness;
            half _Metallic;
            half _WaveContrast;
            half _ReflectionStrength;

            half _WaveHeight;
            half _WaveFrequency;
            half _WaveSpeed;
            half4 _WaveDir;

            half _FoamAmount;
            half _FoamSpeed;
            half _FoamScale;

            void SimpleWave(inout float3 worldPos, inout half3 normal, float time)
            {
                half2 dir = normalize(_WaveDir.xy);
                float phase = dot(dir, worldPos.xz) * _WaveFrequency + time * _WaveSpeed;
                
                float height = sin(phase) * _WaveHeight;
                worldPos.y += height;

                normal.x -= cos(phase) * dir.x * _WaveHeight * 1.2;
                normal.z -= cos(phase) * dir.y * _WaveHeight * 1.2;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);

                SimpleWave(worldPos, worldNormal, _Time.y);

                o.worldPos = worldPos;
                o.worldNormal = normalize(worldNormal);
                
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.screenPos = ComputeScreenPos(o.pos);

                // Simple world-space UV for foam
                o.foamUV = worldPos.xz * _FoamScale;

                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // === FOAM (simple animated noise-like) ===
                float2 foamUV = i.foamUV + _Time.y * _FoamSpeed * 0.5;
                half foam1 = sin(foamUV.x + foamUV.y * 1.3) * 0.5 + 0.5;
                half foam2 = sin(foamUV.x * 1.7 - foamUV.y * 0.8 + _Time.y * _FoamSpeed * 1.3) * 0.5 + 0.5;
                half foamFactor = saturate((foam1 + foam2) * _FoamAmount - 1.2);

                // === LIGHTING ===
                half3 worldNormal = normalize(i.worldNormal);
                half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                half3 halfDir = normalize(lightDir + viewDir);

                half diffuse = saturate(dot(worldNormal, half3(0,1,0))) * 0.5 + 0.5;
                diffuse = pow(diffuse, _WaveContrast);

                half ndh = max(0, dot(worldNormal, halfDir));
                half spec = pow(ndh, _Glossiness * 24.0) * _Metallic;

                // Reflection
                half3 reflectDir = reflect(-viewDir, worldNormal);
                half4 skyCubemap = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflectDir);
                half3 reflection = DecodeHDR(skyCubemap, unity_SpecCube0_HDR) * _ReflectionStrength;

                half fresnel = pow(1.0 - saturate(dot(worldNormal, viewDir)), 2.8);

                // Color
                half depthFactor = 0.4; // fixed for now (you can bring back depth later if needed)
                half3 waterColor = lerp(_ShallowWaterColor.rgb, _DeepWaterColor.rgb, depthFactor) * diffuse;
                half3 baseColor = lerp(waterColor, reflection, fresnel * 0.75) + (spec * _LightColor0.rgb);
                half3 finalColor = lerp(baseColor, _FoamColor.rgb, foamFactor);

                UNITY_APPLY_FOG(i.fogCoord, finalColor);

                half alpha = lerp(_ShallowWaterColor.a, _DeepWaterColor.a, depthFactor);
                return half4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "VRChat/Mobile/Standard Lite"
}