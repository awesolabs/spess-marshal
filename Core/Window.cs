using System.Collections.Generic;
using System.Linq;
using Godot;

public class Window : Open, StaticGeom {
	public override string DatumPath { get; set; } = "/window";

	public override string Icon { get; set; } = "Assets/window.png";

	public override string IconState { get; set; } = "window-255";

	public new Mesh RenderStaticGeom(
		Map grid,
		Vector3 gridpos,
		Atom atom,
		ref SurfaceTool st,
		ref IconMaterial mat
	) {
		this.IconState = "window-255";
		mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaDepthPrePass;
		grid.Data.TryGetValue(key: gridpos + Vector3.Forward, value: out var forward);
		grid.Data.TryGetValue(key: gridpos + Vector3.Back, value: out var backward);
		grid.Data.TryGetValue(key: gridpos + Vector3.Left, value: out var left);
		grid.Data.TryGetValue(key: gridpos + Vector3.Right, value: out var right);
		grid.Data.TryGetValue(key: gridpos + Vector3.Up, value: out var up);
		grid.Data.TryGetValue(key: gridpos + Vector3.Down, value: out var down);
		var uvcoord = new Vector2();
		var uvmap = new Vector2[4] {
			mat.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 0, y: 0), uvcoord: ref uvcoord, dir: this.Dir),
			mat.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 1, y: 0), uvcoord: ref uvcoord, dir: this.Dir),
			mat.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 1, y: 1), uvcoord: ref uvcoord, dir: this.Dir),
			mat.GetUVCoord(state: this.IconState, vertcoord: new Vector2(x: 0, y: 1), uvcoord: ref uvcoord, dir: this.Dir),
		};
		if (forward == null || !isClosed(atoms: forward) && !isWindow(atoms: forward)) {
			MeshHelper.GenCardinalQuad(
				surfaceTool: ref st,
				position: gridpos + Vector3.Forward,
				direction: Vector3.Forward,
				uvmap: uvmap
			);
		}
		if (backward == null || !isClosed(atoms: backward) && !isWindow(atoms: backward)) {
			MeshHelper.GenCardinalQuad(
				surfaceTool: ref st,
				position: gridpos,
				direction: Vector3.Back,
				uvmap: uvmap
			);
		}
		if (left == null || !isClosed(atoms: left) && !isWindow(atoms: left)) {
			MeshHelper.GenCardinalQuad(
				surfaceTool: ref st,
				position: gridpos,
				direction: Vector3.Left,
				uvmap: uvmap
			);
		}
		if (right == null || !isClosed(atoms: right) && !isWindow(atoms: right)) {
			MeshHelper.GenCardinalQuad(
				surfaceTool: ref st,
				position: gridpos + Vector3.Right,
				direction: Vector3.Right,
				uvmap: uvmap
			);
		}
		if (up == null || !isClosed(atoms: up) && !isWindow(atoms: up)) {
			MeshHelper.GenCardinalQuad(
				surfaceTool: ref st,
				position: gridpos + Vector3.Up,
				direction: Vector3.Up,
				uvmap: uvmap
			);
		}
		if (down == null || !isClosed(atoms: down) && !isWindow(atoms: down)) {
			MeshHelper.GenCardinalQuad(
				surfaceTool: ref st,
				position: gridpos,
				direction: Vector3.Down,
				uvmap: uvmap
			);
		}
		return null;
		static bool isClosed(List<Datum> atoms) => atoms?.Any(predicate: a => a is Closed) ?? false;
		static bool isWindow(List<Datum> atoms) => atoms?.Any(predicate: a => a is Window) ?? false;
	}
}