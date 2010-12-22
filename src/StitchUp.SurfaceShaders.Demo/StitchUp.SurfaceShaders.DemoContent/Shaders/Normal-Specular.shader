surface Specular;

[params]
float4 DiffuseColor : DIFFUSE_COLOR = float4(1, 1, 1, 1);
float4 SpecularColor : SPECULAR_COLOR = float4(0.5, 0.5, 0.5, 1);
float SpecularPower : SPECULAR_POWER;

[textures]
Texture2D MainTex : DIFFUSE_TEXTURE;

[vertex]
float2 uv;

[interpolators]
float2 uv;

[lightingmodel BlinnPhong]

[ps]
__hlsl__
void surface(INPUT input, inout BlinnPhongSurfaceOutput output)
{
	float4 c = tex2D(MainTex, input.uv) * DiffuseColor;
	output.Diffuse = c.rgb;
	output.Alpha = c.a;
	output.Specular = SpecularColor;
	output.SpecularPower = SpecularPower;
}
__hlsl__