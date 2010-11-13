# StitchUp

### What is this?

StitchUp is an XNA 4.0 Content Pipeline extension which implements most of the ideas in Shawn Hargreaves' article
[Generating Shaders From HLSL Fragments](http://www.talula.demon.co.uk/hlsl_fragments/hlsl_fragments.html).

### Quick start

1. Download the latest release from the [downloads page](http://github.com/roastedamoeba/stitchup/downloads).
   The zip file contains StitchUp.Content.Pipeline.dll, which is the only one you need.
2. Read the [wiki](http://github.com/roastedamoeba/stitchup/wiki) for information on writing shader fragments
   and how to use the various importers and processors that are included in StitchUp.

### Why should I use StitchUp

* You want to create your effect files from re-usable shader fragments.
* You want to vary the number of lights or textures you use, but without needing to rewrite the shader.
* Because it's new and shiny?

### What does it look like?

Fragment file:

	fragment base_texture;
	
	[textures]
	texture2D color_map;
	
	[vertexattributes]
	float2 uv;
	
	[interpolators]
	float2 uv;
	
	[ps 1_1]
	__hlsl__
	void main(INPUT input, inout OUTPUT output)
	{
		output.color = tex2D(color_map, input.uv);
	}
	__hlsl__

Stitched effect file:

	Shaders\Fragments\VertexTransform.fragment
	Shaders\Fragments\BaseTexture.fragment
	Shaders\Fragments\DetailTexture.fragment

### How to use StitchUp

1. Add `StitchUp.Content.Pipeline.dll` as a reference to your content project.
2. Create fragments and stitched effect files as needed. If you use `.fragment` and `.stitchedeffect`
   as the file extensions, the files should automatically be linked to the correct StitchUp
   importers and processors. You don't need a processor for `.fragment` files.
3. You can either load the stitched effect at runtime (in the same way as you would load a normal effect),
   or you can use the StitchUp model processor to associate the effect with a model at compile-time.
   To do this:
   1. Select the model in your content project.
   2. Change the Content Processor to "Model - StitchUp".
   3. Set the "Stitched Effect" processor property to the path (relative to the root of your content
      project) of your `.stitchedeffect` file.