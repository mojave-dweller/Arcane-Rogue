sampler2D SceneTexture : register(s0);
sampler2D LightMaskTexture : register(s1);

float2 TextureSize;
float3 SunColor;
float SunIntensity;
float AmbientIndoor;

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 scene = tex2D(SceneTexture, texCoord);
    float mask = tex2D(LightMaskTexture, texCoord).a;

    // Outdoor areas get full sun, indoor gets a dim ambient
    float lightAmount = lerp(AmbientIndoor, SunIntensity, mask);
    float3 light = SunColor * lightAmount;

    return float4(scene.rgb * light, scene.a);
}

technique SunLight
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}