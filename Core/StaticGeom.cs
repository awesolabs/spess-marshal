using Godot;

public interface StaticGeom {
	public Mesh RenderStaticGeom(
		Map map,
		Vector3 pos,
		Atom atom,
		ref SurfaceTool st,
		ref IconMaterial mat
	);
}