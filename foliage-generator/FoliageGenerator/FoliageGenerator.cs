using Godot;
using System;

[Tool]
public partial class FoliageGenerator : Node3D
{
	private bool _toolActive = false;
	
	public FastNoiseLite noiseMap;
	public string seed;
	
	public override void _Ready() {
		// process that occurs at runtime
		if (!Engine.IsEditorHint()) {
			_toolActive = false;
			// FIXME: what the attached foliage tool 
			// outputs from the parent object's transform
		}
	}
	
	public override void _Process(double delta) {
		// process that occurs in editor
		if (Engine.IsEditorHint()) {
			// FIXME: foliage tool output preview
		}
	}
}
