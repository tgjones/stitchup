using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public static class Keywords
	{
		private static readonly Dictionary<string, TokenType> KeywordMappings;

		static Keywords()
		{
			KeywordMappings = new Dictionary<string,TokenType>
			{
				{ "fragment", TokenType.Fragment },
				{ "effect", TokenType.Effect },
				{ "float", TokenType.Float },
				{ "float2", TokenType.Float2 },
				{ "float3", TokenType.Float3 },
				{ "float4", TokenType.Float4 },
				{ "matrix", TokenType.Matrix },
				{ "bool", TokenType.Bool },
				{ "texture2D", TokenType.Texture2D },
			};
		}

		public static bool IsKeyword(string value)
		{
			return KeywordMappings.ContainsKey(value);
		}

		public static TokenType GetKeywordType(string value)
		{
			return KeywordMappings[value];
		}
	}
}