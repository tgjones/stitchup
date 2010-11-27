using System;
using System.Collections.Generic;
using System.IO;
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
			HeaderCodeGenerator.GenerateAllHeaderCode(this, _stitchedEffect);
			ParameterGenerator.GenerateAllParameters(this, _stitchedEffect);
			SamplerGenerator.GenerateAllSamplers(this, _stitchedEffect);
			StructGenerator.WriteAllVertexInputStructures(this, _stitchedEffect);
			StructGenerator.WriteAllPixelInputStructures(this, _stitchedEffect);
			StructGenerator.WriteAllVertexOutputStructures(this, _stitchedEffect);
			StructGenerator.WriteAllPixelOutputStructs(this, _stitchedEffect);

			List<ExportedValue> exportedValues = StitchedEffectPreProcessor.GetExportedValues(_stitchedEffect);
			ExportedValueGenerator.GenerateExportDeclarations(this, exportedValues);

			ShaderGenerator.WriteAllVertexShaders(this, _stitchedEffect);
			ShaderGenerator.WriteAllPixelShaders(this, _stitchedEffect);

			TechniqueGenerator.GenerateAllTechniques(this, _stitchedEffect);
		}

		internal string GetVariableDeclaration(StitchedFragmentSymbol stitchedFragment, VariableDeclarationNode variable)
		{
			string arrayStuff = (variable.IsArray && variable.ArraySize != null) ? "[" + variable.ArraySize + "]" : string.Empty;
			string semantic = (!string.IsNullOrEmpty(variable.Semantic)) ? " : " + variable.Semantic : string.Empty;
			string initialValue = (!string.IsNullOrEmpty(variable.InitialValue)) ? " = " + variable.InitialValue : string.Empty;

            return string.Format("{0} {1}_{2}{3}{4}{5};",
				Token.GetString(variable.DataType), stitchedFragment.UniqueName,
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