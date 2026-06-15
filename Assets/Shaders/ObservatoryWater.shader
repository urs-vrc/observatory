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
        _FoamAmount ("Foam Depth", Range(0, 2)) = 0.5
        
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
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma multi_compile_instancing

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
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2; 
                UNITY_FOG_COORDS(3)           
                UNITY_VERTEX_OUTPUT_STEREO
            };

            half4 _ShallowWaterColor;
            half4 _DeepWaterColor;
            half4 _FoamColor;
            half _Glossiness;
            half _Metallic;
            half _FoamAmount;
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

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            
            float getWaveHeight(float2 pos, float time)
            {
                float wave1 = sin(pos.x * _WaveFrequency + time) * _WaveAmplitude;
                float wave2 = sin(pos.y * _WaveFrequency * 0.8 + time * 1.2) * _WaveAmplitude * 0.5;
                return wave1 + wave2;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Apply vertex displacement for ocean volume
                float height = getWaveHeight(worldPos.xz, _Time.y * _WaveSpeed);
                worldPos.y += height;

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

                float rawDepth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos));
                float sceneZ = LinearEyeDepth(rawDepth);
                float surfaceZ = i.screenPos.w; 
                float depthDifference = sceneZ - surfaceZ;
                
                float depthFactor = saturate(depthDifference / 5.0); 
                float foamFactor = saturate(1.0 - (depthDifference / max(0.001, _FoamAmount)));

                float timeSwell = _Time.y * _SwellSpeed;
                float timeChop = _Time.y * _ChopSpeed;

                float2 swellUV1 = i.worldPos.xz * _SwellTiling + float2(timeSwell * 0.1, timeSwell * 0.05);
                float2 swellUV2 = i.worldPos.xz * (_SwellTiling * 0.85) + float2(-timeSwell * 0.07, timeSwell * 0.12);
                float3 swellNorm1 = float3(cos(swellUV1.x + swellUV1.y), sin(swellUV1.y - swellUV1.x), 1.0);
                float3 swellNorm2 = float3(sin(swellUV2.y + swellUV2.x), cos(swellUV2.x - swellUV2.y), 1.0);
                float3 blendedSwell = normalize(float3(swellNorm1.xy + swellNorm2.xy, swellNorm1.z * swellNorm2.z));

                float2 chopUV1 = i.worldPos.xz * _ChopTiling + float2(-timeChop * 0.2, timeChop * 0.15);
                float2 chopUV2 = i.worldPos.xz * (_ChopTiling * 1.5) + float2(timeChop * 0.25, -timeChop * 0.3);
                float3 chopNorm1 = float3(sin(chopUV1.y * 1.2), cos(chopUV1.x * 1.2), 1.0);
                float3 chopNorm2 = float3(cos(chopUV2.x * 0.9), sin(chopUV2.y * 0.9), 1.0);
                float3 blendedChop = normalize(float3(chopNorm1.xy + chopNorm2.xy, chopNorm1.z * chopNorm2.z));

                float3 finalNormal = normalize(float3(
                    (blendedSwell.xy * _SwellScale) + (blendedChop.xy * _ChopScale),
                    blendedSwell.z * blendedChop.z
                ));
                float3 worldNormal = normalize(float3(finalNormal.x, finalNormal.z, finalNormal.y));

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                if (length(lightDir) == 0) lightDir = normalize(float3(0.3, 1.0, 0.2));
                float3 halfDir = normalize(lightDir + viewDir);

                // Generates self-shadowing on the wave slopes so they stand out under monochrome skies
                float waveDiffuse = dot(worldNormal, normalize(float3(0.2, 0.9, 0.1)));
                waveDiffuse = waveDiffuse * 0.5 + 0.5;
                waveDiffuse = pow(waveDiffuse, _WaveContrast);

                float ndh = max(0.0, dot(worldNormal, halfDir));
                float spec = pow(ndh, _Glossiness * 128.0) * _Metallic;

                float3 reflectDir = reflect(-viewDir, worldNormal);
                Unity_GlossyEnvironmentData envData;
                envData.roughness = 1.0 - _Glossiness;
                envData.reflUVW = reflectDir;
                half3 reflection = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
                half3 ambientSkyColor = ShadeSH9(half4(0.0, 1.0, 0.0, 1.0));
                reflection *= ambientSkyColor * _ReflectionStrength;

                float fresnel = pow(1.0 - max(0.0, dot(worldNormal, viewDir)), 5.0);
                
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