using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.Parser
{
	public class FragmentParser
	{
		private readonly StringReader _fragmentReader;

		public FragmentContent Fragment
		{
			get;
			private set;
		}

		public FragmentParser(string fragment)
		{
			_fragmentReader = new StringReader(fragment);
			Fragment = new FragmentContent();
		}

		public void Parse()
		{
			ParseInterfaceBlock();
			SkipWhiteSpace();
			if (_fragmentReader.Peek() == -1)
				return;

			ParseShader();
		}

		#region Interface block

		private void ParseInterfaceBlock()
		{
			SkipWhiteSpace();
			ReadKnownText("interface()");
			SkipWhiteSpace();
			ReadKnownText("{");
			while (_fragmentReader.Peek() != '}')
				ParseInterfaceAssignment();
			SkipWhiteSpace();
			ReadKnownText("}");
		}

		private void ParseInterfaceAssignment()
		{
			SkipWhiteSpace();

			string name = ReadVariableName();

			SkipWhiteSpace();
			ReadKnownText("=");
			SkipWhiteSpace();

			string value = ReadToEndOfLine();

			switch (name)
			{
				case "name":
					Fragment.InterfaceName = value;
					break;
				case "params":
					Fragment.InterfaceParams.Add(value);
					break;
				case "textures" :
					Fragment.InterfaceTextures.Add(value);
					break;
				case "vertex":
					Fragment.InterfaceVertex.Add(value);
					break;
				case "interpolators":
					Fragment.InterfaceInterpolators.Add(value);
					break;
				default :
					// If value contains square brackets, then we have multiple values.
					FragmentParameterContent parameterContent = ParseInterfaceParameterMetadata(value);
					Fragment.InterfaceParameterMetadata.Add(name, parameterContent);
					break;
			}

			SkipWhiteSpace();
		}

		private FragmentParameterContent ParseInterfaceParameterMetadata(string value)
		{
			FragmentParameterContent content = new FragmentParameterContent();
			StringReader metadataReader = new StringReader(value);
			SkipWhiteSpace(metadataReader);
			if (metadataReader.Peek() == '[')
			{
				ReadKnownText(metadataReader, "[");
				SkipWhiteSpace(metadataReader);
				content.DataType = FragmentParameterDataTypeUtility.FromString(ReadUntil(metadataReader, ',', ' ', ']'));
				SkipWhiteSpace(metadataReader);
				while (metadataReader.Peek() != ']')
				{
					ReadKnownText(metadataReader, ",");
					SkipWhiteSpace(metadataReader);
					string metadataName = ReadUntil(metadataReader, '=');
					ReadKnownText(metadataReader, "=");
					string metadataValue = ReadUntil(metadataReader, ',', ' ', ']').Trim('"');
					switch (metadataName)
					{
						case "semantic" :
							content.Semantic = metadataValue;
							break;
						default :
							throw new NotSupportedException();
					}
					SkipWhiteSpace(metadataReader);
				}
			}
			else
			{
				content.DataType = FragmentParameterDataTypeUtility.FromString(ReadContiguousText(metadataReader));
			}
			return content;
		}

		private string ReadVariableName()
		{
			ReadKnownText("$");
			return ReadContiguousText();
		}

		#endregion

		#region Shader

		private void ParseShader()
		{
			SkipWhiteSpace();

			FragmentCodeShaderType shaderType;
			string rawShaderType = ReadContiguousText();
			switch (rawShaderType)
			{
				case "vs":
					shaderType = FragmentCodeShaderType.VertexShader;
					break;
				case "ps":
					shaderType = FragmentCodeShaderType.PixelShader;
					break;
				default:
					throw new ParserException("Expected 'vs' or 'ps'");
			}

			SkipWhiteSpace();

			string version = ReadContiguousText();

			SkipWhiteSpace();

			string code = ReadLinesUntil("vs", "ps").Trim();

			FragmentCodeContent codeContent = new FragmentCodeContent
			{
				ShaderType = shaderType,
				Version = version,
				Code = code
			};

			Fragment.CodeBlocks.Add(codeContent);
		}

		#endregion

		private static string ReadUntil(StringReader reader, params char[] marker)
		{
			string result = string.Empty;
			while (true)
			{
				int nextChar = reader.Peek();
				if (nextChar == -1)
					return result;

				if (marker.Any(m => m == (char)nextChar))
					return result;

				reader.Read();
				result += (char) nextChar;
			}
		}

		private string ReadLinesUntil(params string[] markers)
		{
			string result = string.Empty;
			while (true)
			{
				string line = _fragmentReader.ReadLine();
				if (line == null || markers.Any(m => line.StartsWith(m)))
					return result;

				result += line + Environment.NewLine;
			}
		}

		private static string ReadContiguousText(StringReader reader)
		{
			string result = string.Empty;
			while (true)
			{
				int nextChar = reader.Peek();
				if (nextChar == -1)
					break;

				if (char.IsWhiteSpace((char)nextChar))
					break;

				reader.Read();
				result += (char) nextChar;
			}
			return result;
		}

		private string ReadToEndOfLine()
		{
			return _fragmentReader.ReadLine();
		}

		private string ReadContiguousText()
		{
			return ReadContiguousText(_fragmentReader);
		}

		private static void SkipWhiteSpace(StringReader reader)
		{
			while (char.IsWhiteSpace((char)reader.Peek()))
				reader.Read();
		}

		private void SkipWhiteSpace()
		{
			SkipWhiteSpace(_fragmentReader);
		}

		private static void ReadKnownText(StringReader reader, string text)
		{
			char[] buffer = new char[text.Length];
			reader.Read(buffer, 0, text.Length);

			if (new string(buffer) != text)
				throw new ParserException(string.Format("Expected '{0}'.", text));
		}

		private void ReadKnownText(string text)
		{
			ReadKnownText(_fragmentReader, text);
		}
	}
}