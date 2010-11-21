using System.Collections.Generic;
using StitchUp.Content.Pipeline.FragmentLinking.CodeModel;
using System;

namespace StitchUp.Content.Pipeline.FragmentLinking.Parser
{
	public static class Keywords
	{
		private static readonly Dictionary<string, TokenType> KeywordMappings;

        static Keywords()
        {
            KeywordMappings = new Dictionary<string, TokenType>();
            foreach (TokenType tokenType in Enum.GetValues(typeof(TokenType)))
                KeywordMappings.Add(Token.GetString(tokenType).ToLower(), tokenType);
        }

		public static bool IsKeyword(string value)
		{
            // Keywords should not be case-sensitive
			return KeywordMappings.ContainsKey(value.ToLower());
		}

		public static TokenType GetKeywordType(string value)
		{
            // Keywords should not be case-sensitive
			return KeywordMappings[value.ToLower()];
		}
	}
}