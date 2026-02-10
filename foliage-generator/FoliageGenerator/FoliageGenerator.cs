using Godot;
using System;

[Tool]
public partial class FoliageGenerator : Node3D
{
	private GodotObject discSample;
	private bool _test = true;
	private Vector3 _scalePrev;
	
	[ExportCategory("Active Editor Updates")]
	[Export] public bool toolActive = false;
	
	[ExportCategory("Object Settings")]
	[Export] public GodotObject objectMesh;
	[Export(PropertyHint.Range, "0,10,")]
	public float objectXScale = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float objectYScale = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float objectZScale = 1f;
	
	[ExportCategory("Generation Settings")]
	[Export] public FastNoiseLite noise { get; set; }
	[Export] public int maximumObjects;
	
	[ExportCategory("Seed")]
	[Export] public string seed;
	
	public override void _Ready() {
		// process that occurs at runtime
		if (Engine.IsEditorHint()) {
			noise.Offset = this.GetPosition();
			_scalePrev = this.GetScale();
		}
		
		if (!Engine.IsEditorHint()) {
			toolActive = false;
			// FIXME: what the attached foliage tool 
			// outputs from the parent object's transform
		}
	}
	
	public override void _Process(double delta) {
		// process that occurs in editor
		if (Engine.IsEditorHint() && toolActive) {
			if (noise.Offset != this.GetPosition() || _scalePrev != this.GetScale()) { 
				noise.Offset = this.GetPosition();
				_scalePrev = this.GetScale();
				
				GD.Print("new pos: " + this.GetPosition() + ", new scale: " + this.GetScale());
			}
		}
	}
	
	private void _InstantiateSampling() {
		var scriptPath = GD.Load<GDScript>(
			"res://addons/PoissonDiscSampling/poisson_disc_sampling.gd");	
		discSample = (GodotObject)scriptPath.New();
	}
}
