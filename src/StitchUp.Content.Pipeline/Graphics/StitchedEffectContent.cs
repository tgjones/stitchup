using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace StitchUp.Content.Pipeline.Graphics
{
	public class StitchedEffectContent : ContentItem
	{
		public List<string> Fragments { get; private set; }

		public StitchedEffectContent()
		{
			Fragments = new List<string>();
		}
	}
}