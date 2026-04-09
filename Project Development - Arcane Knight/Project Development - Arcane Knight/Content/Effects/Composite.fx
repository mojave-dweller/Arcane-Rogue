sampler2D SceneTexture : register(s0);
sampler2D SunTexture : register(s1);
sampler2D PointLightTexture : register(s2);

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 scene = tex2D(SceneTexture, texCoord);
    float4 sun = tex2D(SunTexture, texCoord);
    float4 pointLight = tex2D(PointLightTexture, texCoord);

    // Start with sun-lit scene
    float3 lit = sun.rgb;

    // Add point light contribution on top
    lit += scene.rgb * pointLight.rgb * pointLight.a;

    return float4(lit, scene.a);
}

technique Composite
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 MainPS();
    }
}