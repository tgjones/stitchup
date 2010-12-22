using System;
using System.Text;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.FragmentLinking.Generator;

namespace StitchUp.SurfaceShaders.Content.Pipeline.Shaders
{
	public class FragmentWriter
	{
		private readonly StringBuilder _builder;

		public FragmentWriter()
		{
			_builder = new StringBuilder();
		}

		public void WriteDeclaration(string name)
		{
			_builder.AppendLine("fragment " + name + ";");
			_builder.AppendLine();
		}

		public void WriteParameterBlock(string name, ParameterBlockNode block)
		{
			if (block == null)
				return;

			_builder.AppendLine("[" + name + "]");
			foreach (VariableDeclarationNode variableDeclaration in block.VariableDeclarations)
				_builder.AppendLine(EffectCodeGenerator.GetVariableDeclaration(variableDeclaration));

			_builder.AppendLine();
		}

		public void WriteHeaderCode(HeaderCodeBlockNode block)
		{
			_builder.AppendLine("[headercode]");
			_builder.AppendLine(block.Code);
		}

		public void WritePixelShaderCodeBlocks(ShaderCodeBlockNodeCollection blocks, Func<ShaderCodeBlockNode, string> callback)
		{
			foreach (ShaderCodeBlockNode block in blocks)
				WritePixelShaderCodeBlock(block, callback);
		}

		public void WritePixelShaderCodeBlock(ShaderCodeBlockNode block, Func<ShaderCodeBlockNode, string> callback)
		{
			if (block.ShaderProfile != null)
				_builder.AppendLine("[ps " + block.ShaderProfile.GetDescription() + "]");
			else
				_builder.AppendLine("[ps]");
			_builder.AppendLine(callback(block));
		}

		public override string ToString()
		{
			return _builder.ToString();
		}
	}
}