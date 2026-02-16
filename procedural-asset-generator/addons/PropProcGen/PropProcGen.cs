using Godot;
using System;

[Tool]
public partial class PropProcGen : Node3D
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
	[ExportGroup("Object Density")]
	[Export(PropertyHint.Range, "0,20000,")]
	public int maximumObjects;
	[ExportGroup("Noise")]
	[Export] public FastNoiseLite fastNoiseLite { get; set; }
	[Export(PropertyHint.Range, "0,30,")]
	public float noiseScalar;
	[ExportGroup("Jitter")]
	[Export(PropertyHint.Range, "0,0.1,")]
	public float jitterUpperBound;
	[Export(PropertyHint.Range, "-0.1,0,")]
	public float jitterLowerBound;
	[ExportGroup("X-Axis Scalar")]
	[Export] public bool scaleX;
	[Export(PropertyHint.Range, "0,10,")]
	public float xValueScalar = 1f;
	[ExportGroup("Y-Axis Scalar")]
	[Export] public bool scaleY;
	[Export(PropertyHint.Range, "0,10,")]
	public float yValueScalar = 1f;
	[ExportGroup("Z-Axis Scalar")]
	[Export] public bool scaleZ;
	[Export(PropertyHint.Range, "0,10,")]
	public float zValueScalar = 1f;
	[ExportGroup("X-Axis Rotation")]
	[Export] public bool rotateX;
	[Export(PropertyHint.Range, "-180,180,")]
	public float xRotateUpperBound;
	[Export(PropertyHint.Range, "-180,180,")]
	public float xRotateLowerBound;
	[ExportGroup("Y-Axis Rotation")]
	[Export] public bool rotateY;
	[Export(PropertyHint.Range, "-180,180,")]
	public float yRotateUpperBound;
	[Export(PropertyHint.Range, "-180,180,")]
	public float yRotateLowerBound;
	[ExportGroup("Z-Axis Rotation")]
	[Export] public bool rotateZ;
	[Export(PropertyHint.Range, "-180,180,")]
	public float zRotateUpperBound;
	[Export(PropertyHint.Range, "-180,180,")]
	public float zRotateLowerBound;
	
	[ExportCategory("Culling Settings")]
	[ExportGroup("X-Axis Culling")]
	[Export] public bool cullXValues;
	[Export(PropertyHint.Range, "0,10,")]
	public float xCullingMinimum = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float xCullingMaximum = 1f;
	[ExportGroup("Y-Axis Culling")]
	[Export] public bool cullYValues;
	[Export(PropertyHint.Range, "0,10,")]
	public float yCullingMinimum = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float yCullingMaximum = 1f;
	[ExportGroup("Z-Axis Culling")]
	[Export] public bool cullZValues;
	[Export(PropertyHint.Range, "0,10,")]
	public float zCullingMinimum = 1f;
	[Export(PropertyHint.Range, "0,10,")]
	public float zCullingMaximum = 1f;
	
	[ExportCategory("Seed")]
	[Export] public string seed;
	
	public override void _Ready() {
		if (!Engine.IsEditorHint()) {
			// FIXME: what the attached foliage tool 
			// outputs from the parent object's transform
		}
	}
	
	public override void _Process(double delta) {
		if (Engine.IsEditorHint()) {
			_CheckCullRanges();
			_CheckRotationBounds();
		}
	}
	
	private void _PopulateObjectArray() {
		if (_objects != null) {
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
		int maxIter = (int)Math.Floor(Math.Sqrt(maximumObjects));
		int arrayIter = 0;
		
		for (int j = (int)(-(maxIter / 2)); j < (int)(maxIter / 2); j++) {
			for (int i = (int)(-(maxIter / 2)); i < (int)(maxIter / 2); i++) {
				MeshInstance3D currObj = _objects[arrayIter];
				float currNoise = fastNoiseLite.GetNoise2D(
					(float)(this.GetScale().X * i / Math.Sqrt(maximumObjects)),
					(float)(this.GetScale().Z * j / Math.Sqrt(maximumObjects))
				);
				
				_SetObjectScale(currObj, currNoise);
				_SetObjectPosition(currObj, i, j);
				_SetObjectRotation(currObj);
				
				_TryCullObject(currObj);
				arrayIter++;
			}
		}
	}
	
	private void _SetObjectScale(MeshInstance3D obj, float noise) {
		Vector3 scaleVec;
		
		scaleVec = obj.GetScale() * new Vector3(
			scaleX ? Math.Abs(xValueScalar * noise * noiseScalar) : 1,
			scaleY ? Math.Abs(yValueScalar * noise * noiseScalar) : 1,
			scaleZ ? Math.Abs(zValueScalar * noise * noiseScalar) : 1
		);
		obj.SetScale(scaleVec);
	}
	
	private void _SetObjectPosition(MeshInstance3D obj, int i, int j) {
		Vector3 posVec;
		var rng = new RandomNumberGenerator();
		
		posVec = new Vector3((float)((i / Math.Sqrt(maximumObjects)) + rng.RandfRange(jitterLowerBound, jitterUpperBound)), 
			(float)this.Position[1] + (float)Math.Abs(obj.Scale[1] / 2), 
			(float)((j / Math.Sqrt(maximumObjects)) + rng.RandfRange(jitterLowerBound, jitterUpperBound)));
		obj.SetPosition(ToGlobal(posVec));
	}
	
	private void _SetObjectRotation(MeshInstance3D obj) {
		var rng = new RandomNumberGenerator();
		
		if (rotateX) {
			obj.RotateX(rng.RandfRange(xRotateLowerBound, xRotateUpperBound) * (float)(Math.PI/180));
		}
		if (rotateY) {
			obj.RotateY(rng.RandfRange(yRotateLowerBound, yRotateUpperBound) * (float)(Math.PI/180));
		}
		if (rotateZ) {
			obj.RotateZ(rng.RandfRange(zRotateLowerBound, zRotateUpperBound) * (float)(Math.PI/180));
		}
	}
	
	private void _TryCullObject(MeshInstance3D obj) {
		if (cullXValues) {
			if (obj.GetScale().X > xCullingMinimum && obj.GetScale().X < xCullingMaximum) {
				Owner.AddChild(obj);
			}
		}
		else if (cullYValues) {
			if (obj.GetScale().Y > yCullingMinimum && obj.GetScale().Y < yCullingMaximum) {
				Owner.AddChild(obj);
			}
		}
		else if (cullZValues) {
			if (obj.GetScale().Z > zCullingMinimum && obj.GetScale().Z < zCullingMaximum) {
				Owner.AddChild(obj);
			}
		}
		else { Owner.AddChild(obj); }
	}
	
	private void _CheckCullRanges() {
		if (xCullingMinimum >= xCullingMaximum) {
			cullXValues = false;
			xCullingMinimum = 0;
			xCullingMaximum = 10;
		}
		if (yCullingMinimum >= yCullingMaximum) {
			cullYValues = false;
			yCullingMinimum = 0;
			yCullingMaximum = 10;
		}
		if (zCullingMinimum >= zCullingMaximum) {
			cullZValues = false;
			zCullingMinimum = 0;
			zCullingMaximum = 10;
		}
	}
	
	private void _CheckRotationBounds() {
		if (xRotateLowerBound >= xRotateUpperBound) { xRotateLowerBound = xRotateUpperBound; } 
		if (yRotateLowerBound >= yRotateUpperBound) { yRotateLowerBound = yRotateUpperBound; } 
		if (zRotateLowerBound >= zRotateUpperBound) { zRotateLowerBound = zRotateUpperBound; } 
	}
	
	public void Generate() {
		if (Engine.IsEditorHint()) {
			_CheckCullRanges();
			_CheckRotationBounds();
			
			fastNoiseLite.Offset = this.GetPosition();
			_PopulateObjectArray();
			_PopulateToolArea();
		}
	}
}
