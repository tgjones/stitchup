using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using StitchUp.Content.Pipeline.Graphics;
using StitchUp.Content.Pipeline.Properties;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public class StitchedEffectParser : Parser
	{
		private readonly ContentIdentity _stitchedFragmentContentIdentity;

		public StitchedEffectParser(string path, Token[] tokens, ContentIdentity stitchedFragmentContentIdentity)
			: base(path, tokens)
		{
			_stitchedFragmentContentIdentity = stitchedFragmentContentIdentity;
		}

		public StitchedEffectNode Parse()
		{
			TokenIndex = 0;

			return ParseStitchedEffectDeclaration();
		}

		private StitchedEffectNode ParseStitchedEffectDeclaration()
		{
			IdentifierToken fragmentName = ParseFileDeclaration(TokenType.StitchedEffect);

			List<ParseNode> blocks = ParseBlocks();

			return new StitchedEffectNode
			{
				Name = fragmentName.Identifier,
				Fragments = blocks.OfType<FragmentBlockNode>().FirstOrDefault(),
				Techniques = blocks.OfType<TechniqueBlockNode>().FirstOrDefault(),
				HeaderCode = blocks.OfType<HeaderCodeBlockNode>().FirstOrDefault(),
			};
		}

		protected override Func<ParseNode> GetBlockParseMethod(IdentifierToken blockName)
		{
			switch (blockName.Identifier)
			{
				case "techniques":
					return () => ParseTechniqueBlock();
				case "fragments":
					return () => ParseFragmentsBlock();
				default:
					return base.GetBlockParseMethod(blockName);
			}
		}

		private ParseNode ParseTechniqueBlock()
		{
			return new TechniqueBlockNode
			{
				Techniques = new List<TechniqueNode>()
			};
		}

		private ParseNode ParseFragmentsBlock()
		{
			Eat(TokenType.CloseSquare);

			List<VariableDeclarationNode> variableDeclarations = ParseVariableDeclarations(true, true, false, false, TokenType.Fragment);
			return new FragmentBlockNode
			{
				FragmentDeclarations = variableDeclarations.ToDictionary(
					vd => vd.Name,
					vd => new ExternalReference<FragmentContent>(vd.InitialValue.Trim('"'), _stitchedFragmentContentIdentity))
			};
		}

		protected override ParameterBlockType GetParameterBlockType(IdentifierToken blockName)
		{
			ReportError(Resources.StitchedEffectParserParameterBlockTypeExpected, blockName.Identifier);
			throw new NotSupportedException();
		}
	}
}