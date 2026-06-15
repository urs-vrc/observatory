Shader "Observatory/ObservatoryWater"
{
    Properties
    {
        [Header(Color and Transparency)]
        _ShallowWaterColor ("Shallow Water Color", Color) = (0.1, 0.4, 0.5, 0.85)
        _DeepWaterColor ("Deep Water Color", Color) = (0.05, 0.12, 0.2, 0.95)
        _Glossiness ("Smoothness", Range(0,1)) = 0.95
        _Metallic ("Metallic", Range(0,1)) = 0.6
        _WaveContrast ("Overcast Wave Contrast", Range(0.5, 4)) = 1.8
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 1.0

        [Header(Vertex Waves)]
        _WaveAmplitude ("Wave Height", Range(0, 2)) = 0.2
        _WaveFrequency ("Wave Frequency", Float) = 0.2
        _WaveSpeed ("Wave Speed", Float) = 1.0

        [Header(Shoreline Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount ("Foam Intensity", Range(0, 2)) = 1.0
        _FoamThreshold ("Foam Threshold", Range(0, 1)) = 0.5

        [Header(Normal Micro Detail)]
        _SwellScale ("Swell Strength", Range(0, 1)) = 0.4
        _SwellTiling ("Swell Size (Lower = Bigger)", Float) = 0.02
        _SwellSpeed ("Swell Speed", Float) = 0.3

        _ChopScale ("Chop Strength", Range(0, 1)) = 0.25
        _ChopTiling ("Chop Size (Lower = Bigger)", Float) = 0.15
        _ChopSpeed ("Chop Speed", Float) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Back

        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 worldPos : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
                half2 waveGradient : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                UNITY_FOG_COORDS(5)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _ShallowWaterColor;
            half4 _DeepWaterColor;
            half4 _FoamColor;
            half _Glossiness;
            half _Metallic;
            half _FoamAmount;
            half _FoamThreshold;
            half _WaveContrast;
            half _ReflectionStrength;

            half _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;

            half _SwellScale;
            float _SwellTiling;
            float _SwellSpeed;

            half _ChopScale;
            float _ChopTiling;
            float _ChopSpeed;

            // Simple hash for noise
            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
            }

            // 2D value noise
            float noise(float2 p)
            {
                float2 ip = floor(p);
                float2 u = frac(p);
                u = u * u * (3.0 - 2.0 * u);

                float a = rand(ip);
                float b = rand(ip + float2(1.0, 0.0));
                float c = rand(ip + float2(0.0, 1.0));
                float d = rand(ip + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float getWaveHeight(float2 pos, float time)
            {
                float wave1 = sin(pos.x * _WaveFrequency + time * _WaveSpeed);
                float wave2 = sin(pos.y * _WaveFrequency * 0.8 + time * _WaveSpeed * 1.2) * 0.5;
                return (wave1 + wave2) * _WaveAmplitude;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float time = _Time.y;

                // Apply vertex displacement
                float height = getWaveHeight(worldPos.xz, time);
                worldPos.y += height;

                // Compute wave gradient for foam (slope)
                float dHdx = cos(worldPos.x * _WaveFrequency + time * _WaveSpeed) * _WaveFrequency * _WaveAmplitude;
                float dHdz = cos(worldPos.z * _WaveFrequency * 0.8 + time * _WaveSpeed * 1.2) * _WaveFrequency * 0.8 * 0.5 * _WaveAmplitude;
                o.waveGradient = half2(dHdx, dHdz);

                // Pass world normal for lighting
                o.worldNormal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.worldPos = worldPos;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.pos);

                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                half waveSlope = length(i.waveGradient);
                half foamFactor = saturate((waveSlope - _FoamThreshold) * _FoamAmount);
                foamFactor *= saturate(noise(i.worldPos.xz * 2.0) * 0.5 + 0.5); // Add noise for detail
                
                float2 swellUV1 = i.worldPos.xz * _SwellTiling + _Time.y * _SwellSpeed * float2(0.1, 0.05);
                float2 swellUV2 = i.worldPos.xz * (_SwellTiling * 0.85) + _Time.y * _SwellSpeed * float2(-0.07, 0.12);
                float swellNoise1 = noise(swellUV1) * 2.0 - 1.0;
                float swellNoise2 = noise(swellUV2) * 2.0 - 1.0;
                half3 swellNormal = normalize(half3(swellNoise1, swellNoise2, 1.0));

                float2 chopUV1 = i.worldPos.xz * _ChopTiling + _Time.y * _ChopSpeed * float2(-0.2, 0.15);
                float2 chopUV2 = i.worldPos.xz * (_ChopTiling * 1.5) + _Time.y * _ChopSpeed * float2(0.25, -0.3);
                float chopNoise1 = noise(chopUV1) * 2.0 - 1.0;
                float chopNoise2 = noise(chopUV2) * 2.0 - 1.0;
                half3 chopNormal = normalize(half3(chopNoise1, chopNoise2, 1.0));

                // Combine normals with wave slope
                half3 finalNormal = normalize(
                    i.worldNormal +
                    (swellNormal * _SwellScale) +
                    (chopNormal * _ChopScale) +
                    half3(i.waveGradient, 0.0) * 0.1
                );
                
                half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                if (length(_WorldSpaceLightPos0.xyz) < 0.01) lightDir = normalize(half3(0.3, 1.0, 0.2));
                half3 halfDir = normalize(lightDir + viewDir);

                // Wave diffuse
                half waveDiffuse = dot(finalNormal, half3(0.2, 0.9, 0.1));
                waveDiffuse = pow(saturate(waveDiffuse * 0.5 + 0.5), _WaveContrast);

                // Specular
                half ndh = max(0.0, dot(finalNormal, halfDir));
                half spec = pow(ndh, _Glossiness * 128.0) * _Metallic;
                
                half3 reflectDir = reflect(-viewDir, finalNormal);
                #ifdef UNITY_PASS_TEXCUBE
                Unity_GlossyEnvironmentData envData;
                envData.roughness = 1.0 - _Glossiness;
                envData.reflUVW = reflectDir;
                half3 reflection = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData) * _ReflectionStrength;
                #else
                half3 reflection = _ReflectionStrength * ShadeSH9(half4(0.0, 1.0, 0.0, 1.0)); // Fallback
                #endif
                
                half fresnel = pow(1.0 - max(0.0, dot(finalNormal, viewDir)), 5.0);
                half depthFactor = saturate(i.worldPos.y / 5.0);
                
                half3 waterColor = lerp(_ShallowWaterColor.rgb, _DeepWaterColor.rgb, depthFactor);
                waterColor *= waveDiffuse;
                half3 baseColor = lerp(waterColor, reflection, fresnel) + (spec * _LightColor0.rgb);
                half3 finalColor = lerp(baseColor, _FoamColor.rgb, foamFactor);

                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                return half4(finalColor, lerp(_ShallowWaterColor.a, _DeepWaterColor.a, depthFactor));
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}