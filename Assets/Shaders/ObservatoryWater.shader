Shader "Observatory/ObservatoryWater"
{
    Properties
    {
        [Header(Color and Transparency)]
        _ShallowWaterColor ("Shallow Water Color", Color) = (0.1, 0.4, 0.5, 0.85)
        _DeepWaterColor ("Deep Water Color", Color) = (0.05, 0.12, 0.2, 0.95)
        _Glossiness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.5
        _WaveContrast ("Wave Shading Contrast", Range(0.5, 4)) = 1.5
        _ReflectionStrength ("Reflection Strength", Range(0, 1)) = 0.5

        [Header(Wave 1)]
        _Wave1Height ("Swell Height", Range(0, 2)) = 0.3
        _Wave1Length ("Swell Frequency", Float) = 0.04
        _Wave1Speed ("Swell Speed", Float) = 1.0
        _Wave1Steepness ("Swell Sharpness (Gerstner)", Range(0, 1)) = 0.5
        _Wave1Dir ("Swell Direction", Vector) = (1.0, 0.1, 0, 0)

        [Header(Wave 2)]
        _Wave2Height ("Chop Height (Set 0 to Disable)", Range(0, 2)) = 0.05
        _Wave2Length ("Chop Frequency (Higher = Smaller)", Float) = 0.2
        _Wave2Speed ("Chop Speed", Float) = 2.0
        _Wave2Steepness ("Chop Sharpness", Range(0, 1)) = 0.7
        _Wave2Dir ("Chop Direction", Vector) = (-0.4, 0.8, 0, 0)

        [Header(Shoreline Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamAmount ("Foam Depth", Range(0, 2)) = 0.4
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

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
                half3 waveNormal : TEXCOORD3;
                UNITY_FOG_COORDS(2)           
                UNITY_VERTEX_INPUT_INSTANCE_ID
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

            half _Wave1Height;
            half _Wave1Length;
            half _Wave1Speed;
            half _Wave1Steepness;
            half4 _Wave1Dir;

            half _Wave2Height;
            half _Wave2Length;
            half _Wave2Speed;
            half _Wave2Steepness;
            half4 _Wave2Dir;

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            // Fast, single-pass analytical Gerstner Wave calculation
            void ComputeGerstnerWave(float4 waveDir, float height, float length, float speed, float steepness, float3 worldPos, inout float3 displace, inout float3 tangent, inout float3 binormal)
            {
                half2 d = normalize(waveDir.xy);
                float k = length;
                float phase = dot(d, worldPos.xz) * k + _Time.y * speed;
                
                float sinP = sin(phase);
                float cosP = cos(phase);

                // Control horizontal packing factor based on height to avoid loops/self-intersection
                float q = steepness / (k * max(0.001, height));

                displace.x += q * height * d.x * cosP;
                displace.y += height * sinP;
                displace.z += q * height * d.y * cosP;

                // Accumulate partial derivatives for structural vectors
                tangent += float3(-q * d.x * d.x * k * height * sinP, d.x * k * height * cosP, -q * d.x * d.y * k * height * sinP);
                binormal += float3(-q * d.x * d.y * k * height * sinP, d.y * k * height * cosP, -q * d.y * d.y * k * height * sinP);
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                float3 displace = float3(0, 0, 0);
                float3 tangent = float3(1, 0, 0);
                float3 binormal = float3(0, 0, 1);


                if (_Wave1Height > 0.0)
                {
                    ComputeGerstnerWave(_Wave1Dir, _Wave1Height, _Wave1Length, _Wave1Speed, _Wave1Steepness, worldPos, displace, tangent, binormal);
                }


                if (_Wave2Height > 0.0)
                {
                    ComputeGerstnerWave(_Wave2Dir, _Wave2Height, _Wave2Length, _Wave2Speed, _Wave2Steepness, worldPos, displace, tangent, binormal);
                }
                
                worldPos += displace;
                
                // Cross-product calculation keeps normals perfectly aligned with geometric peaks
                o.waveNormal = normalize(cross(binormal, tangent));
                o.worldPos = worldPos;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.screenPos = ComputeScreenPos(o.pos);

                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 sceneUV = i.screenPos.xy / max(0.0001, i.screenPos.w);
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sceneUV);
                float sceneZ = LinearEyeDepth(rawDepth);
                float surfaceZ = i.screenPos.w; 
                
                half depthDifference = (half)(sceneZ - surfaceZ);
                half depthFactor = saturate(depthDifference * 0.2); 
                half foamFactor = saturate(1.0 - (depthDifference / max(0.001, _FoamAmount)));
                
                half3 worldNormal = normalize(i.waveNormal);

                half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                half3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                half3 halfDir = normalize(lightDir + viewDir);

                half waveDiffuse = saturate(dot(worldNormal, half3(0.0, 1.0, 0.0))) * 0.4 + 0.6;
                waveDiffuse = pow(waveDiffuse, _WaveContrast);

                half ndh = max(0.0, dot(worldNormal, halfDir));
                half spec = pow(ndh, _Glossiness * 32.0) * _Metallic; 
                
                half3 reflectDir = reflect(-viewDir, worldNormal);
                half mip = (1.0 - _Glossiness) * 7.0;
                half4 skyCubemap = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectDir, mip);
                half3 reflection = DecodeHDR(skyCubemap, unity_SpecCube0_HDR) * _ReflectionStrength;

                half fresnel = pow(1.0 - max(0.0, dot(worldNormal, viewDir)), 3.0);
                
                half3 waterColor = lerp(_ShallowWaterColor.rgb, _DeepWaterColor.rgb, depthFactor) * waveDiffuse;
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