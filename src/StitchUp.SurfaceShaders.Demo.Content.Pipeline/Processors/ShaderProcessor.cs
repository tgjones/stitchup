using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Parser;
using StitchUp.SurfaceShaders.Content.Pipeline.Processors;
using StitchUp.SurfaceShaders.Content.Pipeline.Shaders;

namespace StitchUp.SurfaceShaders.Demo.Content.Pipeline.Processors
{
	[ContentProcessor(DisplayName = "Shader - StitchUp Demo")]
	public class ShaderProcessor : ShaderProcessorBase
	{
		protected override string BuildStitchedEffectFile(ShaderContent input)
		{
			// First generate the .fragment file. This file looks similar to the .shader file,
			// but adds on a few more things that are necessary for StitchUp.
			string generatedFragmentFile, lightingModel;
			GenerateFragmentFile(input, out generatedFragmentFile, out lightingModel);

			string pathPrefix = Directory.GetParent(Path.GetDirectoryName(input.Identity.SourceFilename)).FullName + @"\";

			return
				string.Format(
					@"stitchedeffect GeneratedEffect;

[fragments]
fragment lc = ""{0}Fragments\Lights\LightCommon.fragment"";
fragment lm = ""{0}Fragments\LightingModels\{2}.fragment"";
fragment pnt = ""{0}Fragments\VertexTypes\PositionNormalTexture.fragment"";
fragment vpt = ""{0}Fragments\VertexTypes\VertexPassThru.fragment"";
fragment surf = ""{1}"";
fragment pco = ""{0}Fragments\PixelColorOutput.fragment"";

[techniques]
technique t1
{{
	pass p1
	{{
		fragments = [lc, pnt, vpt, ""{0}Fragments\Lights\DirectionalLight.fragment"", surf, pco];
	}}
}}

technique t2
{{
	pass p1
	{{
		fragments = [lc, pnt, vpt, ""{0}Fragments\Lights\PointLight.fragment"", surf, pco];
	}}
}}", pathPrefix, generatedFragmentFile, lightingModel);
		}

		private void GenerateFragmentFile(ShaderContent input,
			out string generatedFragmentFile,
			out string lightingModel)
		{
			lightingModel = input.ShaderNode.LightingModel.LightingModel;

			FragmentWriter writer = new FragmentWriter();
			writer.WriteDeclaration("GeneratedFragment");

			input.ShaderNode.Parameters.VariableDeclarations.Add(new VariableDeclarationNode
			{
				Name = "CameraPosition",
				Semantic = "CAMERA_POSITION",
				DataType = TokenType.Float3
			});
			input.ShaderNode.Parameters.VariableDeclarations.Add(new VariableDeclarationNode
			{
				Name = "AmbientColor",
				Semantic = "AMBIENT_COLOR",
				DataType = TokenType.Float4
			});
			writer.WriteParameterBlock("params", input.ShaderNode.Parameters);
			writer.WriteParameterBlock("interpolators", input.ShaderNode.Interpolators);
			writer.WriteParameterBlock("textures", input.ShaderNode.Textures);
			writer.WriteParameterBlock("vertex", input.ShaderNode.VertexAttributes);
			writer.WriteParameterBlock("pixeloutputs", input.ShaderNode.PixelOutputs);

			if (input.ShaderNode.HeaderCode != null)
				writer.WriteHeaderCode(input.ShaderNode.HeaderCode);

			// Shader code.
			string tempLightingModel = lightingModel;
			writer.WritePixelShaderCodeBlocks(input.ShaderNode.PixelShaders,
				b => GetPixelShaderCode(tempLightingModel, b));

			// Save to temporary .stitchedeffect file.
			generatedFragmentFile = GetTempFileName(input, ".fragment");
			File.WriteAllText(generatedFragmentFile, writer.ToString(), Encoding.GetEncoding(1252));
		}

		private static string GetPixelShaderCode(string lightingModel, ShaderCodeBlockNode shaderCodeBlock)
		{
			const string surfaceOutputStructNamePattern = @"void surface\(INPUT input, inout (?<STRUCTNAME>[\w]+) output\)";
			string surfaceOutputStructName =
				new Regex(surfaceOutputStructNamePattern).Match(shaderCodeBlock.Code).Groups["STRUCTNAME"].Value;

			string newCode = string.Format(@"void main(INPUT input, inout OUTPUT output)
{{
	float3 worldNormal;
	import(_WorldNormal, worldNormal = _WorldNormal);

	float3 worldPosition;
	import(_WorldPosition, worldPosition = _WorldPosition);

	{0} s = ({0}) 0;
	s.Normal = worldNormal;
	surface(input, s);

	float3 viewDirection = normalize(CameraPosition - worldPosition);

	// Import whatever lights have been exported by previous light fragments,
	// and process them one-by-one. Because light is additive, the final intensity of the light
	// reflected by a given surface is simply the sum of the ambient, diffuse and specular components.
	float4 finalColor = AmbientColor;
	import(_Light, finalColor += Lighting{1}(s, _Light, 1.0, viewDirection));

	export(float4, _LightColor, finalColor);
}}", surfaceOutputStructName, lightingModel);

			string oldCode = shaderCodeBlock.Code;

			return Lexer.HlslDelimiter 
				+ oldCode
				+ Environment.NewLine 
				+ Environment.NewLine
				+ newCode 
				+ Environment.NewLine 
				+ Lexer.HlslDelimiter;
		}
	}
}