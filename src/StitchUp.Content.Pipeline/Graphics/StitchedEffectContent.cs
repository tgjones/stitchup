using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace StitchUp.Content.Pipeline.Graphics
{
	public class StitchedEffectContent : ContentItem
	{
		public List<ExternalReference<FragmentContent>> Fragments { get; private set; }

		public StitchedEffectContent()
		{
		    Fragments = new List<ExternalReference<FragmentContent>>();
		}
	}
}