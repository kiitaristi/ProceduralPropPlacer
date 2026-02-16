using Godot;

[Tool]
public partial class PropProcGenPlugin : EditorPlugin
{
	public override void _EnterTree() {
		var script = GD.Load<Script>("res://addons/PropProcGen/PropProcGen.cs");
		var icon = GD.Load<Texture2D>("res://addons/PropProcGen/apg_icon_transp.png");
		
		AddCustomType("ProceduralPropNode", "Node3D", script, icon);
	}
	
	public override void _ExitTree() {
		RemoveCustomType("ProceduralPropNode");
	}
}
