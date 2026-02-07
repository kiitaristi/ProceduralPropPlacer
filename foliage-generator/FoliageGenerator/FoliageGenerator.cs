using Godot;
using System;

[Tool]
public partial class FoliageGenerator : Node3D
{
	private bool _toolActive = false;
	private GodotObject discSample; 
	
	[ExportCategory("Mesh Settings")]
	[Export] public MeshInstance3D meshObject;
	[Export(PropertyHint.Range, "0,10,")]
	public float meshScale = 1f;
	
	[ExportCategory("Noise Settings")]
	[Export] public FastNoiseLite noiseMap;
	[Export] public int maximumObjects;
	
	[ExportCategory("Seed")]
	[Export] public string seed;
	
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
			
		}
	}
	
	private void _InstantiateSampling() {
		var scriptPath = GD.Load<GDScript>();
	}
}
