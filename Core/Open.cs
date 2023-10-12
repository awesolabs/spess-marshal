using Godot;

public class Open : Atom, StaticGeom {
	public override string DatumPath { get; set; } = "/open";

	public override string Icon { get; set; } = "Assets/floors.png";

	public override string IconState { get; set; } = "plating";

	public override int Dir { get; set; } = 2;

	public Mesh RenderStaticGeom(
		Map map,
		Vector3 gridpos,
		Atom atom,
		ref SurfaceTool st,
		ref IconMaterial icon
	) {
		var uvcoord = new Vector2();
		var uvmap = new Vector2[4] {
			icon.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 0, y: 0), uvcoord: ref uvcoord, dir: this.Dir),
			icon.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 1, y: 0), uvcoord: ref uvcoord, dir: this.Dir),
			icon.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 1, y: 1), uvcoord: ref uvcoord, dir: this.Dir),
			icon.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 0, y: 1), uvcoord: ref uvcoord, dir: this.Dir),
		};
		MeshHelper.GenCardinalQuad(
			surfaceTool: ref st,
			position: gridpos,
			direction: Vector3.Up,
			uvmap: uvmap
		);
		MeshHelper.GenCardinalQuad(
			surfaceTool: ref st,
			position: gridpos,
			direction: Vector3.Down,
			uvmap: uvmap
		);
		return null;
	}
}