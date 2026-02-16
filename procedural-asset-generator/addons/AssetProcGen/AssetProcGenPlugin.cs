using Godot;

[Tool]
public partial class AssetProcGenPlugin : EditorPlugin
{
	public override void _EnterTree() {
		var script = GD.Load<Script>("res://addons/AssetProcGen/AssetProcGen.cs");
		var icon = GD.Load<Texture2D>("res://addons/AssetProcGen/apg_icon_transp.png");
		
		AddCustomType("ProceduralAssetNode", "Node3D", script, icon);
	}
	
	public override void _ExitTree() {
		RemoveCustomType("ProceduralAssetNode");
	}
}
