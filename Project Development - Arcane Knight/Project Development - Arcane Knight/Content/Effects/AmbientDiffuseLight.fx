sampler2D DiffuseSampler : register(s0);

float3 AmbientColor = float3(1.0, 1.0, 1.0);
float AmbientIntensity = 0.2;

struct Light
{
    float2 Position;
    float2 Radius_Intensity; // x = Radius, y = Intensity
    float3 Color;
    float Padding;
};

#define MAX_LIGHTS 20
float2 LightPositions[MAX_LIGHTS];
float3 LightColors[MAX_LIGHTS];
float2 LightRadiusIntensity[MAX_LIGHTS];
float LightCount;

float4x4 WorldViewProjection;

struct VertexInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float2 WorldPosition : TEXCOORD1;
};

PixelInput VS_Main(VertexInput input)
{
    PixelInput output;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    output.WorldPosition = input.Position.xy;
    return output;
}

float3 CalcLight(float2 pos, float3 col, float2 ri, float2 worldPos)
{
    float2 toLight = pos - worldPos;
    float distance = length(toLight);
    float attenuation = saturate(1.0 - (distance / ri.x));
    attenuation = attenuation * attenuation;
    return col * ri.y * attenuation;
}

float4 PS_Main(PixelInput input) : SV_TARGET
{
    float4 diffuseSample = tex2D(DiffuseSampler, input.TexCoord);
    clip(diffuseSample.a - 0.01);

    float3 lighting = AmbientColor * AmbientIntensity;

    lighting += CalcLight(LightPositions[0], LightColors[0], LightRadiusIntensity[0], input.WorldPosition);
    lighting += CalcLight(LightPositions[1], LightColors[1], LightRadiusIntensity[1], input.WorldPosition);
    lighting += CalcLight(LightPositions[2], LightColors[2], LightRadiusIntensity[2], input.WorldPosition);
    lighting += CalcLight(LightPositions[3], LightColors[3], LightRadiusIntensity[3], input.WorldPosition);
    lighting += CalcLight(LightPositions[4], LightColors[4], LightRadiusIntensity[4], input.WorldPosition);
    lighting += CalcLight(LightPositions[5], LightColors[5], LightRadiusIntensity[5], input.WorldPosition);
    lighting += CalcLight(LightPositions[6], LightColors[6], LightRadiusIntensity[6], input.WorldPosition);
    lighting += CalcLight(LightPositions[7], LightColors[7], LightRadiusIntensity[7], input.WorldPosition);
    lighting += CalcLight(LightPositions[8], LightColors[8], LightRadiusIntensity[8], input.WorldPosition);
    lighting += CalcLight(LightPositions[9], LightColors[9], LightRadiusIntensity[9], input.WorldPosition);
    lighting += CalcLight(LightPositions[10], LightColors[10], LightRadiusIntensity[10], input.WorldPosition);
    lighting += CalcLight(LightPositions[11], LightColors[11], LightRadiusIntensity[11], input.WorldPosition);
    lighting += CalcLight(LightPositions[12], LightColors[12], LightRadiusIntensity[12], input.WorldPosition);
    lighting += CalcLight(LightPositions[13], LightColors[13], LightRadiusIntensity[13], input.WorldPosition);
    lighting += CalcLight(LightPositions[14], LightColors[14], LightRadiusIntensity[14], input.WorldPosition);
    lighting += CalcLight(LightPositions[15], LightColors[15], LightRadiusIntensity[15], input.WorldPosition);
    lighting += CalcLight(LightPositions[16], LightColors[16], LightRadiusIntensity[16], input.WorldPosition);
    lighting += CalcLight(LightPositions[17], LightColors[17], LightRadiusIntensity[17], input.WorldPosition);
    lighting += CalcLight(LightPositions[18], LightColors[18], LightRadiusIntensity[18], input.WorldPosition);
    lighting += CalcLight(LightPositions[19], LightColors[19], LightRadiusIntensity[19], input.WorldPosition);

    return float4(diffuseSample.rgb * saturate(lighting), diffuseSample.a) * input.Color;
}

technique AmbientDiffuseLight
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Main();
    }
}