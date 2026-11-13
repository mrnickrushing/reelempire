// The harbour's water: a deep-to-shallow colour toward the shore, moving ripples that catch the sun, a foam line
// along the quay, darker and glossier at night. Opaque (no scene textures needed), so it is cheap on phones.
Shader "ReelEmpire/Water"
{
    Properties
    {
        _Deep ("Deep", Color) = (0.05, 0.27, 0.42, 1)
        _Shallow ("Shallow", Color) = (0.16, 0.56, 0.62, 1)
        _Foam ("Foam", Color) = (0.92, 0.97, 1, 1)
        _ShoreX ("Shore x", Float) = 0
        _ShoreFade ("Shallow reach", Float) = 38
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Deep, _Shallow, _Foam;
                float _ShoreX, _ShoreFade;
            CBUFFER_END
            float _NightGlow;

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float fogFactor : TEXCOORD1; };

            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
            }
            float waves(float2 p, float t)
            {
                float h = sin(p.x * 0.35 + t * 1.1) * 0.5 + sin(p.y * 0.42 - t * 0.9 + p.x * 0.12) * 0.5;
                h += noise(p * 0.55 + float2(t * 0.35, -t * 0.22)) * 1.2 + noise(p * 1.7 - float2(t * 0.5, t * 0.3)) * 0.45;
                return h;
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                o.positionWS = ws;
                o.positionCS = TransformWorldToHClip(ws);
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float t = _Time.y;
                float2 p = i.positionWS.xz;
                float e = 0.35;
                float h0 = waves(p, t), hx = waves(p + float2(e, 0), t), hz = waves(p + float2(0, e), t);
                float3 n = normalize(float3((h0 - hx) * 0.22, 1, (h0 - hz) * 0.22));

                float shore = saturate((_ShoreX - i.positionWS.x) / _ShoreFade);     // 0 at the quay, 1 out at sea
                float3 col = lerp(_Shallow.rgb, _Deep.rgb, smoothstep(0, 1, shore));
                col *= 0.9 + 0.12 * h0 * 0.5;

                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                float ndl = saturate(dot(n, light.direction));
                float day = 1 - _NightGlow;
                float3 lit = col * (lerp(0.22, 0.5, day) + ndl * light.color * lerp(0.55, 0.75, light.shadowAttenuation));
                // The sky in the water, strongest at a glancing angle.
                float fres = pow(1 - saturate(dot(n, v)), 4);
                float3 sky = lerp(float3(0.06, 0.09, 0.2), float3(0.62, 0.78, 0.92), day);
                lit = lerp(lit, sky, fres * 0.55);
                // Sun glitter.
                float3 hv = normalize(light.direction + v);
                lit += light.color * pow(saturate(dot(n, hv)), 180) * 1.6 * light.shadowAttenuation;
                // Foam: a moving line against the quay and soft caps on the bigger ripples.
                float edge = saturate(1 - (_ShoreX - i.positionWS.x) / 2.2);
                float lap = smoothstep(0.45, 0.9, edge + noise(p * 1.3 + float2(0, t * 0.6)) * 0.5 - 0.15) * step(i.positionWS.x, _ShoreX);
                float caps = smoothstep(1.75, 2.1, h0) * 0.35;
                lit = lerp(lit, _Foam.rgb * lerp(0.35, 1, day), saturate(lap * 0.8 + caps));
                lit = MixFog(lit, i.fogFactor);
                return half4(lit, 1);
            }
            ENDHLSL
        }
    }
}
