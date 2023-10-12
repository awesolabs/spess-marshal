using System.Linq;
using Godot;

public class MeshHelper {
	/// <summary>
	///     Generates a cardinal quad in clockwise winding order in the form of:
	///     A(0, 1, 0)   B(1, 1, 0)
	///     ┌──────────┐
	///     │          │
	///     │          │
	///     └──────────┘
	///     D(0, 0, 0)  C(1, 0, 0)
	///     Mesh UV should be supplied with a Vector2[4] in the form of:
	///     A(0,0)        B(1,0)
	///     ┌──────────┐
	///     │          │
	///     │          │
	///     │          │
	///     └──────────┘
	///     D(0,1)        C(1,1)
	///     UVs should be preadjusted for your material prior to generation.
	/// </summary>
	/// <param name="surfaceTool">The surface tool to apply Verts and UVs to</param>
	/// <param name="position">Position relative to unit origin to offset generation</param>
	/// <param name="direction">Cardinal direction Vector3.(UP,DOWN,LEFT,RIGHT,FRONT,BACK)</param>
	/// <param name="uvmap">uv values</param>
	public static void GenCardinalQuad(
		ref SurfaceTool surfaceTool,
		Vector3 position,
		Vector3 direction,
		Vector2[] uvmap
	) {
		var unitverts = new Vector3[] {
			new(x: 0, y: 1, z: 0), //A
			new(x: 1, y: 1, z: 0), //B
			new(x: 1, y: 0, z: 0), //C
			new(x: 0, y: 0, z: 0), //D
		};
		var offset = Vector3.Zero;
		if (direction == Vector3.Up) {
			unitverts = unitverts
				.Select(selector: a => a.Rotated(axis: new Vector3(x: 1, y: 0, z: 0), angle: Mathf.DegToRad(deg: -90)))
				.ToArray();
		}
		else if (direction == Vector3.Down) {
			offset = Vector3.Forward;
			unitverts = unitverts
				.Select(selector: a => a.Rotated(axis: new Vector3(x: 1, y: 0, z: 0), angle: Mathf.DegToRad(deg: 90)))
				.ToArray();
		}
		else if (direction == Vector3.Left) {
			offset = Vector3.Forward;
			unitverts = unitverts
				.Select(selector: a => a.Rotated(axis: new Vector3(x: 0, y: 1, z: 0), angle: Mathf.DegToRad(deg: -90)))
				.ToArray();
		}
		else if (direction == Vector3.Right) {
			unitverts = unitverts
				.Select(selector: a => a.Rotated(axis: new Vector3(x: 0, y: 1, z: 0), angle: Mathf.DegToRad(deg: 90)))
				.ToArray();
		}
		else if (direction == Vector3.Forward) {
			offset = Vector3.Right;
			unitverts = unitverts
				.Select(selector: a => a.Rotated(axis: new Vector3(x: 0, y: 1, z: 0), angle: Mathf.DegToRad(deg: 180)))
				.ToArray();
		}
		var pos = position;
		unitverts = unitverts
			.Select(selector: a => a + pos + offset)
			.ToArray();
		//A
		surfaceTool.SetUV(uvmap[0]);
		surfaceTool.AddVertex(unitverts[0]);
		//B
		surfaceTool.SetUV(uvmap[1]);
		surfaceTool.AddVertex(unitverts[1]);
		//C
		surfaceTool.SetUV(uvmap[2]);
		surfaceTool.AddVertex(unitverts[2]);
		//C
		surfaceTool.SetUV(uvmap[2]);
		surfaceTool.AddVertex(unitverts[2]);
		//D
		surfaceTool.SetUV(uvmap[3]);
		surfaceTool.AddVertex(unitverts[3]);
		//A
		surfaceTool.SetUV(uvmap[0]);
		surfaceTool.AddVertex(unitverts[0]);
	}
}