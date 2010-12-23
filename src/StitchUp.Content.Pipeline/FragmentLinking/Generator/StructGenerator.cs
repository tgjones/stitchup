using System.Linq;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.EffectModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Generator
{
	public static class StructGenerator
	{
		#region Pixel output

		public static void WriteAllPixelOutputStructs(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachPass(WritePixelOutputStructs);
		}

		private static void WritePixelOutputStructs(EffectCodeGenerator generator,
			TechniqueSymbol technique, TechniquePassSymbol pass, string uniquePassName)
		{
			ScriptTextWriter writer = generator.Writer;
			writer.WriteLine("// -------- pixel output structures --------");

			SemanticGenerator semanticGenerator = new SemanticGenerator("COLOR", 1);
			pass.ForEachFragment(generator, (g, f, n) => WritePixelOutputStructure(g, f, n, semanticGenerator));

			writer.WriteLine("struct {0}_PIXELOUTPUT", uniquePassName);
			writer.WriteLine("{");
			writer.WriteLine("\tfloat4 color : COLOR0;");
			pass.ForEachFragment(generator, (g, f, s) =>
			{
				if (f.FragmentNode.PixelOutputs != null && f.FragmentNode.PixelOutputs.VariableDeclarations.Any())
					writer.WriteLine("\t{0}_{1}_PIXELOUTPUT {1};", s, f.UniqueName);
			});
			writer.WriteLine("};");
			writer.WriteLine();
		}

		private static void WritePixelOutputStructure(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment, string uniqueName, SemanticGenerator semanticGenerator)
		{
			WriteShaderInputStructure(generator, stitchedFragment, uniqueName, semanticGenerator, "PIXELOUTPUT", stitchedFragment.FragmentNode.PixelOutputs, true);
		}

		#endregion

		#region Pixel input

		public static void WriteAllPixelInputStructures(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachPass(WritePixelInputStructs);
		}

		private static void WritePixelInputStructs(EffectCodeGenerator generator, 
			TechniqueSymbol technique, TechniquePassSymbol pass, string uniquePassName)
		{
			generator.Writer.WriteLine("// -------- pixel input structures --------");

			SemanticGenerator semanticGenerator = new SemanticGenerator("TEXCOORD");
			pass.ForEachFragment(generator, (g, f, s) => WritePixelInputStructure(g, f, s, semanticGenerator));

			generator.Writer.WriteLine("struct {0}_PIXELINPUT", uniquePassName);
			generator.Writer.WriteLine("{");
			pass.ForEachFragment(generator, (g, f, s) => g.Writer.WriteLine("\t{0}_{1}_PIXELINPUT {1};", s, f.UniqueName));
			generator.Writer.WriteLine("};");
			generator.Writer.WriteLine();
			generator.Writer.WriteLine("static {0}_PIXELINPUT gPixelInput_{0};", uniquePassName);
			generator.Writer.WriteLine();
		}

		private static void WritePixelInputStructure(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment, string uniqueName, SemanticGenerator semanticGenerator)
		{
			WriteShaderInputStructure(generator, stitchedFragment, uniqueName, semanticGenerator, "PIXELINPUT", stitchedFragment.FragmentNode.Interpolators, true);
		}

		#endregion

		#region Vertex input

		public static void WriteAllVertexInputStructures(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachPass(WriteVertexInputStructures);
		}

		private static void WriteVertexInputStructures(EffectCodeGenerator generator, 
			TechniqueSymbol technique, TechniquePassSymbol pass, string uniquePassName)
		{
			ScriptTextWriter writer = generator.Writer;

			writer.WriteLine("// -------- vertex input structures --------");

			SemanticGenerator semanticGenerator = new SemanticGenerator("TEXCOORD");
			pass.ForEachFragment(generator, (g, f, s) => WriteVertexInputStructure(g, f, s, semanticGenerator));

			writer.WriteLine("struct {0}_VERTEXINPUT", uniquePassName);
			writer.WriteLine("{");

			pass.ForEachFragment(generator, (g, f, s) => g.Writer.WriteLine("\t{0}_{1}_VERTEXINPUT {1};", uniquePassName, f.UniqueName));

			writer.WriteLine("};");
			writer.WriteLine();
			writer.WriteLine("static {0}_VERTEXINPUT gVertexInput_{0};", uniquePassName);
			writer.WriteLine();
		}

		private static void WriteVertexInputStructure(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment, string uniqueName, SemanticGenerator semanticGenerator)
		{
			WriteShaderInputStructure(generator, stitchedFragment, uniqueName, semanticGenerator, "VERTEXINPUT", stitchedFragment.FragmentNode.VertexAttributes, false);
		}

		#endregion

		#region Vertex output

		public static void WriteAllVertexOutputStructures(EffectCodeGenerator generator, StitchedEffectSymbol stitchedEffect)
		{
			generator.ForEachPass(WriteVertexOutputStructs);
		}

		private static void WriteVertexOutputStructs(EffectCodeGenerator generator, 
			TechniqueSymbol technique, TechniquePassSymbol pass, string uniquePassName)
		{
			ScriptTextWriter writer = generator.Writer;

			writer.WriteLine("// -------- vertex output structures --------");
			writer.WriteLine("struct {0}_VERTEXOUTPUT", uniquePassName);
			writer.WriteLine("{");
			writer.WriteLine("\tfloat4 position : POSITION;");
			pass.ForEachFragment(generator, (g, f, s) =>
			{
				if (f.FragmentNode.Interpolators != null && f.FragmentNode.Interpolators.VariableDeclarations.Any())
					g.Writer.WriteLine("\t{0}_{1}_PIXELINPUT {1};", uniquePassName, f.UniqueName);
			});
			writer.WriteLine("};");
			writer.WriteLine();
		}

		#endregion

		private static void WriteShaderInputStructure(EffectCodeGenerator generator, StitchedFragmentSymbol stitchedFragment, string uniqueName, SemanticGenerator semanticGenerator, string structSuffix,
			ParameterBlockNode parameterBlock, bool alwaysUseTexCoords)
		{
			WriteShaderStructure(generator, stitchedFragment, uniqueName, semanticGenerator, structSuffix, parameterBlock);
		}

		private static void WriteShaderStructure(EffectCodeGenerator generator,
			StitchedFragmentSymbol stitchedFragment, string uniquePassName, SemanticGenerator semanticGenerator,
			string structSuffix, ParameterBlockNode parameterBlock)
		{
			generator.Writer.WriteLine("struct {0}_{1}_{2}", uniquePassName, stitchedFragment.UniqueName, structSuffix);
			generator.Writer.WriteLine("{");

			if (parameterBlock != null)
				parameterBlock.VariableDeclarations.ForEach(v =>
				{
					string semantic = semanticGenerator.GetNextSemantic(v);
					generator.Writer.WriteLine("\t{0} {1} : {2};",
						Token.GetString(v.DataType), v.Name, semantic);
				});

			generator.Writer.WriteLine("};");
			generator.Writer.WriteLine();
		}
	}
}