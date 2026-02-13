using Godot;
using System;

[Tool]
public partial class FoliageGenerator : Node3D
{
	private Vector3 _scalePrev;
	private int _maxObjPrev;
	private Godot.Collections.Array<MeshInstance3D> _objects;
	
	[ExportToolButton("Run Generation")]
	public Callable GenButton => Callable.From(Generate);
	
	[ExportCategory("Object Settings")]
	[Export] public Mesh objectMesh;
	[ExportGroup("Object Scaling")]
	[Export(PropertyHint.Range, "0,10,")]
	public float objectXScale = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float objectYScale = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float objectZScale = 1f;
	
	[ExportCategory("Generation Settings")]
	[ExportGroup("Noise Settings")]
	[Export] public FastNoiseLite noise { get; set; }
	[ExportGroup("Density Settings")]
	[Export(PropertyHint.Range, "0,2000,")]
	public int maximumObjects;
	[ExportGroup("X Scalar Settings")]
	[Export] public bool scaleXValues;
	[Export(PropertyHint.Range, "0,10,")]
	public float xValueScalar = 1f;
	[ExportGroup("Y Scalar Settings")]
	[Export] public bool scaleYValues;
	[Export(PropertyHint.Range, "0,10,")]
	public float yValueScalar = 1f;
	[ExportGroup("Z Scalar Settings")]
	[Export] public bool scaleZValues;
	[Export(PropertyHint.Range, "0,10,")]
	public float zValueScalar = 1f;
	
	[ExportCategory("Seed")]
	[Export] public string seed;
	
	public override void _Ready() {
		// process that occurs at runtime or on editor startup
		if (Engine.IsEditorHint()) {
			noise.Offset = this.GetPosition();
			_PopulateObjectArray();
			_PopulateToolArea();
			
			GD.Print("pos: " + this.GetPosition() + ", scale: " + this.GetScale());
			GD.Print("array size: " + _objects.Count);
		}
		
		if (!Engine.IsEditorHint()) {
			// FIXME: what the attached foliage tool 
			// outputs from the parent object's transform
		}
	}
	
	private void _PopulateObjectArray() {
		if (_objects.Count != 0) {
			foreach (MeshInstance3D obj in _objects) {
				obj.QueueFree();
			}
		}
		_objects = [];
		Vector3 scalarVec = new Vector3(objectXScale, objectYScale, objectZScale);
		
		for (int i = 0; i < maximumObjects; i++) {
			var newObject = new MeshInstance3D();
			newObject.SetMesh(objectMesh);
			newObject.SetScale(newObject.GetScale() * scalarVec);
			
			_objects.Add(newObject);
		}
	}
	
	private void _PopulateToolArea() {
		Vector3 posVec;
		Vector3 scaleVec;
		
		for (int j = 0; j < (int)Math.Floor(Math.Sqrt(maximumObjects)); j++) {
			for (int i = 0; i < (int)Math.Floor(Math.Sqrt(maximumObjects)); i++) {
				float currNoise = noise.GetNoise2D(
					(float)(this.GetScale()[0] * i / Math.Sqrt(maximumObjects)),
					(float)(this.GetScale()[2] * j / Math.Sqrt(maximumObjects))
					);
				GD.Print("noise map " + currNoise);
				MeshInstance3D currObj = _objects[i + j];
					
				posVec = this.GetScale() * new Vector3((float)(i / Math.Sqrt(maximumObjects)), 
					(float)this.Position[1], (float)(j / Math.Sqrt(maximumObjects)));
				currObj.SetPosition(posVec);
					
				scaleVec = currObj.GetScale() * new Vector3(
					scaleXValues ? xValueScalar * currNoise : 1,
					scaleYValues ? yValueScalar * currNoise : 1,
					scaleZValues ? zValueScalar * currNoise : 1
				);
				currObj.SetScale(scaleVec);
				currObj.Show();
				
				GD.Print("i step: " + i);
				GD.Print(currObj.GetType() + " position " + currObj.GetPosition() 
				+ " scale " + currObj.GetScale());
			}
			GD.Print("j step: " + j);
		}
	}
	
	public void Generate() {
		if (Engine.IsEditorHint()) {
			noise.Offset = this.GetPosition();
			_PopulateObjectArray();
			_PopulateToolArea();
			
			foreach (MeshInstance3D obj in _objects) {
				AddChild(obj);
			}
			
			GD.Print("pos: " + this.GetPosition() + ", scale: " + this.GetScale());
			GD.Print("array size: " + _objects.Count);
		}
	}
}
