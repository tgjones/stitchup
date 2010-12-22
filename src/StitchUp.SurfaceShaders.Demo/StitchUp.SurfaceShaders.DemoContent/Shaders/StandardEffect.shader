effect StandardEffect;

__hlsl__
matrix wvp;

float4 vs(float3 position : POSITION) : POSITION
{
	return mul(float4(position, 1), wvp);
}

technique
{
	pass
	{
		VertexShader = compile vs_2_0 vs();
	}
}
__hlsl__