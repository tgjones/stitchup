using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;
using StitchUp.Content.Pipeline.FragmentLinking.PreProcessor;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public class EffectCodeGenerator
	{
		private readonly StitchedEffectSymbol _stitchedEffect;
		private readonly ShaderProfile _targetShaderProfile;
		private readonly GeneratorContext _context;
		private readonly ScriptTextWriter _output;

		public ScriptTextWriter Writer
		{
			get { return _output; }
		}

		public GeneratorContext Context
		{
			get { return _context; }
		}

		public EffectCodeGenerator(
			StitchedEffectSymbol stitchedEffect, ShaderProfile targetShaderProfile,
			TextWriter writer)
		{
			_stitchedEffect = stitchedEffect;
			_targetShaderProfile = targetShaderProfile;
			_output = new ScriptTextWriter(writer);
			_context = new GeneratorContext
			{
				TargetShaderProfile = _targetShaderProfile
			};
		}

		public void GenerateCode()
		{
			// If there are no vertex shaders in any of the fragments, disable vertex shader generation.
			// This is useful, for example, in post processing shaders where SpriteBatch is used.
			bool generateVertexShader = _stitchedEffect.StitchedFragments.Any(f => f.FragmentNode.VertexShaders.Any());

			HeaderCodeGenerator.GenerateAllHeaderCode(this, _stitchedEffect);
			ParameterGenerator.GenerateAllParameters(this, _stitchedEffect);
			SamplerGenerator.GenerateAllSamplers(this, _stitchedEffect);

			if (generateVertexShader)
				StructGenerator.WriteAllVertexInputStructures(this, _stitchedEffect);
			StructGenerator.WriteAllPixelInputStructures(this, _stitchedEffect);

			if (generateVertexShader)
				StructGenerator.WriteAllVertexOutputStructures(this, _stitchedEffect);

			StructGenerator.WriteAllPixelOutputStructs(this, _stitchedEffect);

			List<ExportedValue> exportedValues = StitchedEffectPreProcessor.GetExportedValues(_stitchedEffect);
			ExportedValueGenerator.GenerateExportDeclarations(this, exportedValues);

			if (generateVertexShader)
				ShaderGenerator.WriteAllVertexShaders(this, _stitchedEffect);
			ShaderGenerator.WriteAllPixelShaders(this, _stitchedEffect);

			TechniqueGenerator.GenerateAllTechniques(this, _stitchedEffect, generateVertexShader);
		}

		internal string GetVariableDeclaration(StitchedFragmentSymbol stitchedFragment, VariableDeclarationNode variable)
		{
			return GetVariableDeclaration(variable, stitchedFragment.UniqueName + "_");
		}

		public static string GetVariableDeclaration(VariableDeclarationNode variable, string prefix = "")
		{
			string arrayStuff = (variable.IsArray && variable.ArraySize != null) ? "[" + variable.ArraySize + "]" : string.Empty;
			string semantic = (!string.IsNullOrEmpty(variable.Semantic)) ? " : " + variable.Semantic : string.Empty;

			string initialValue;
			if (!string.IsNullOrEmpty(variable.InitialValue))
			{
				string variableInitialValue = variable.InitialValue;
				if (variableInitialValue.StartsWith("sampler_state"))
					variableInitialValue = Regex.Replace(variableInitialValue, @"(Texture=\()([\w]+)(\);)", "$1" + prefix + "$2$3");
				initialValue = " = " + variableInitialValue;
			}
			else
				initialValue = string.Empty;

			return string.Format("{0} {1}{2}{3}{4}{5};",
				Token.GetString(variable.DataType), prefix,
				variable.Name, arrayStuff, semantic, initialValue);
		}

		internal void ForEachFragment(Action<EffectCodeGenerator, StitchedFragmentSymbol> action)
		{
			foreach (StitchedFragmentSymbol stitchedFragmentNode in _stitchedEffect.StitchedFragments)
				action(this, stitchedFragmentNode);
		}

		internal void ForEachPass(Action<EffectCodeGenerator, TechniqueSymbol, TechniquePassSymbol, string> action)
		{
			foreach (TechniqueSymbol technique in _stitchedEffect.Techniques)
				foreach (TechniquePassSymbol pass in technique.Passes)
					action(this, technique, pass, string.Format("{0}_{1}", technique.Name, pass.Name));
		}
	}
}