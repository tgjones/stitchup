using System;
using System.Collections.Generic;
using StitchUp.Content.Pipeline.Graphics;

namespace StitchUp.Content.Pipeline.FragmentLinking.CodeGeneration
{
	public class FragmentContentCollection : List<FragmentContent>
	{
		public FragmentContentCollection(IEnumerable<FragmentContent> items)
			: base(items)
		{
			
		}

		public void ForEach(Action<FragmentContent, string> predicate)
		{
			for (int i = 0; i < Count; ++i)
				predicate(this[i], this[i].InterfaceName + i);
		}
	}
}