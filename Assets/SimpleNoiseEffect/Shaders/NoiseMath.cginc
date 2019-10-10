#ifndef __NOISEMATH_CGINC__
#define __NOISEMATH_CGINC__

float rand(float x)
{
    return frac(sin(x) * 43758.5453);
}

float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float rand(float3 co)
{
    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 56.787))) * 43758.5453);
}

float2x2 rot(float a)
{
    float s = sin(a);
    float c = cos(a);
    return float2x2(c, -s, s, c);
}

float noise(float3 pos)
{
    float3 ip = floor(pos);
    float3 fp = smoothstep(0, 1, frac(pos));
    float4 a = float4(
        rand(ip + float3(0, 0, 0)),
        rand(ip + float3(1, 0, 0)),
        rand(ip + float3(0, 1, 0)),
        rand(ip + float3(1, 1, 0)));
    float4 b = float4(
        rand(ip + float3(0, 0, 1)),
        rand(ip + float3(1, 0, 1)),
        rand(ip + float3(0, 1, 1)),
        rand(ip + float3(1, 1, 1)));

    a = lerp(a, b, fp.z);
    a.xy = lerp(a.xy, a.zw, fp.y);
    return lerp(a.x, a.y, fp.x);
}

float perlin(float3 pos)
{
    return
        (noise(pos) * 32 +
         noise(pos * 2) * 16 +
         noise(pos * 4) * 8 +
         noise(pos * 8) * 4 +
         noise(pos * 16) * 2 +
         noise(pos * 32)) / 63;
}

/// パーリンノイズによるベクトル場
/// 3Dとして3要素を計算。
/// それぞれのノイズは明らかに違う（極端に大きなオフセット）を持たせた値とする
float3 Pnoise(float3 vec)
{
    float x = perlin(vec);

    float y = perlin(float3(
        vec.y + 31.416,
        vec.z - 47.853,
        vec.x + 12.793
    ));

    float z = perlin(float3(
        vec.z - 233.145,
        vec.x - 113.408,
        vec.y - 185.31
    ));

    return float3(x, y, z);
}

float3 SamplePotential(float3 vec)
{
    return Pnoise(vec);
}

float3 CurlNoise(float3 pos)
{
    const float e = 1e-4f;
    const float e2 = 2.0 * e;
    const float invE2 = 1.0 / e2;

    const float3 dx = float3(e, 0.0, 0.0);
    const float3 dy = float3(0.0, e, 0.0);
    const float3 dz = float3(0.0, 0.0, e);

    float3 p_x0 = SamplePotential(pos - dx);
    float3 p_x1 = SamplePotential(pos + dx);
    float3 p_y0 = SamplePotential(pos - dy);
    float3 p_y1 = SamplePotential(pos + dy);
    float3 p_z0 = SamplePotential(pos - dz);
    float3 p_z1 = SamplePotential(pos + dz);

    float x = (p_y1.z - p_y0.z) - (p_z1.y - p_z0.y);
    float y = (p_z1.x - p_z0.x) - (p_x1.z - p_x0.z);
    float z = (p_x1.y - p_x0.y) - (p_y1.x - p_y0.x);

    return float3(x, y, z) * invE2;
}

#endif