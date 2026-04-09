sampler2D LightMaskTexture : register(s0);

float2 TextureSize;
float2 LightPosition;     // in world space pixels
float3 LightColor;
float LightRadius;
float LightIntensity;
float2 CameraOffset;      // top-left corner of camera in world space
float CameraZoom;

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Convert texCoord to world space pixel position
    float2 screenPos = texCoord * TextureSize;
    float2 worldPos = screenPos / CameraZoom + CameraOffset;

    // Sample the light mask at this world position
    float2 maskCoord = worldPos / TextureSize;
    float mask = tex2D(LightMaskTexture, maskCoord).a;

    // Distance falloff
    float dist = distance(worldPos, LightPosition);
    float falloff = 1.0 - saturate(dist / LightRadius);
    falloff = falloff * falloff; // quadratic falloff

    // Occlude: if we're inside a wall (mask == 0) reduce light
    // This is a soft approximation -- not true raycast shadow
    float occlusion = saturate(mask + 0.15); // small bleed for softness
    float lightAmount = falloff * LightIntensity * occlusion;

    return float4(LightColor * lightAmount, lightAmount);
}

technique PointLight
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}