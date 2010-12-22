surface Diffuse;

[params]
float4 Color : DIFFUSE_COLOR = float4(1, 1, 1, 1);

[textures]
Texture2D MainTex : DIFFUSE_TEXTURE;

[vertex]
float2 uv;

[interpolators]
float2 uv;

[lightingmodel Lambert]

[ps]
__hlsl__
void surface(INPUT input, inout LambertSurfaceOutput output)
{
	float4 c = tex2D(MainTex, input.uv) * Color;
	output.Diffuse = c.rgb;
	output.Alpha = c.a;
}
__hlsl__