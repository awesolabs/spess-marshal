using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;
using FileAccess = Godot.FileAccess;

public partial class Map : Node3D {
	public Dictionary<Vector3, List<Datum>> Data = new();

	public List<string> UnmappedAtoms = new();

	public void PushData(Vector3 key, Datum item) {
		if (!this.Data.TryGetValue(key: key, value: out var value)) {
			value = new List<Datum>();
			this.Data[key] = value;
		}
		value.Add(item);
	}

	public static Map FromDMM(string path, List<Type> ignore = default) {
		Log.Instance.Print(message: $"Loading DMM Map '{path}'");
		var file = FileAccess.Open(
			path: path,
			flags: FileAccess.ModeFlags.Read
		);
		if (file == null) {
			Log.Instance.Print(
				message: $"failed to open map file: {FileAccess.GetOpenError()}"
			);
		}
		var mapdata = file.GetAsText(skipCr: true);
		var stream = new MemoryStream(buffer: Encoding.UTF8.GetBytes(s: mapdata));
		var parser = new Parser(source: stream);
		var result = parser.Parse();
		if (result.Error != null) {
			Log.Instance.Print(
				message: $"ERROR LOADING MAP {result.Error.Message}"
			);
		}
		else {
			Log.Instance.Print(
				message: "DMM Map Loaded and Parsed"
			);
		}
		var map = result.Map;
		var grid = new Map();
		foreach (var chunk in map.Chunks) {
			for (var rowidx = 0 ; rowidx < chunk.Rows.Count ; rowidx++) {
				var gridpos = new Vector3(
					x: (float)chunk.X - 1,
					y: (float)chunk.Z - 1,
					z: -(chunk.Y - 1 + ((float)chunk.Rows.Count - 1 - rowidx))
				);
				var row = chunk.Rows[index: rowidx];
				var prefab = map.Prefab[key: row.Prefab.First()];
				foreach (var atom in prefab.Atoms) {
					if (atom.Path.StartsWith("/area/space")) {
						continue;
					}
					if (atom.Path.StartsWith(value: "/obj/machinery/light")) {
						atom.Path = "/obj/machinery/light";
					}
					if (atom.Path.Contains(value: "/obj/effect/spawner/structure/window/reinforced")) {
						atom.Path = "/window";
					}
					else if (atom.Path.Contains(value: "/obj/effect/spawner/structure/window")) {
						atom.Path = "/window";
					}
					else if (atom.Path.Contains(value: "/obj/effect/spawner/structure/window/reinforced/plasma")) {
						atom.Path = "/window";
					}
					else if (atom.Path.StartsWith(value: "/turf/open/floor") && !ResourceMap.Instance.Has(path: atom.Path)) {
						atom.Path = "/turf/open/floor/plating";
					}
					else if (atom.Path.StartsWith(value: "/obj/effect/landmark/event_spawn")) {
						atom.Path = "/spawn";
					}
					else if (atom.Path.StartsWith(value: "/turf/closed/wall")) {
						atom.Path = "/turf/closed";
					}
					var resource = ResourceMap.Instance.Get(path: atom.Path);
					if (resource == null) {
						grid.UnmappedAtoms.Add(item: atom.Path);
						continue;
					}
					if (Activator.CreateInstance(resource.GetType()) is not Atom inst) {
						Log.Instance.Print(
							message: $"Expecting Atom instance for '{atom.Path}' but duplicate returned null"
						);
						continue;
					}
					if (ignore.Contains(resource.GetType())) {
						continue;
					}
					atom.Props.ToList().ForEach(a => inst.SetProp(name: a.Key, value: a.Value.Value));
					if (!grid.Data.TryGetValue(key: gridpos, value: out var value)) {
						grid.Data[key: gridpos] = new List<Datum>();
					}
					grid.Data[key: gridpos].Add(item: inst);
				}
			}
		}
		grid.UnmappedAtoms = grid.UnmappedAtoms
			.GroupBy(keySelector: a => a)
			.OrderBy(keySelector: a => a.Count())
			.Select(selector: a => $"{a.Key} ({a.Count()})")
			.ToList();
		return grid;
	}

	public void ApplyMappingMutations() {
		var mutations = new List<Action>();
		foreach (var cell in this.Data) {
			foreach (var atom in cell.Value) {
				switch (atom) {
					case Closed closed:
						mutations.Add(
							item: () => this.PushData(
								key: cell.Key + Vector3.Up,
								item: Activator.CreateInstance(closed.GetType()) as Atom
							)
						);
						break;
					case Window window:
						mutations.Add(
							item: () => this.PushData(
								key: cell.Key + Vector3.Up,
								item: Activator.CreateInstance(window.GetType()) as Atom
							)
						);
						break;
					case SpessMarshal.Core.Turfs.open.Floor floor:
						mutations.Add(
							item: () => this.PushData(
								key: cell.Key + (Vector3.Up * 2),
								item: Activator.CreateInstance(typeof(SpessMarshal.Core.Turfs.open.floor.Plating)) as Atom
							)
						);
						break;
					default:
						break;
				}
			}
		}
		mutations.ForEach(action: m => m());
	}

	public void ApplyLights() {
		foreach (var cell in this.Data) {
			foreach (var atom in cell.Value) {
				if (atom is not Light light) {
					continue;
				}
				var light3d = new OmniLight3D {
					Position = cell.Key + Vector3.Up,
					LightBakeMode = Light3D.BakeMode.Static
				};
				this.AddChild(light3d);
			}
		}
	}

	public MeshInstance3D BuildMesh() {
		try {
			var grid = this;
			Log.Instance.Print(message: $"Cells: {grid.Data.Count}");
			Log.Instance.Print(message: $"Datums: {grid.Data.Sum(selector: a => a.Value.Count)}");
			var surfaces = new Dictionary<string, SurfaceTool>();
			var materials = new Dictionary<string, IconMaterial>();
			var staticatoms = new Dictionary<Vector3, List<Atom>>();
			foreach (var cell in grid.Data) {
				foreach (var datum in cell.Value) {
					if (datum is Atom && datum is StaticGeom) {
						if (!staticatoms.TryGetValue(key: cell.Key, value: out var value)) {
							value = new List<Atom>();
							staticatoms[key: cell.Key] = value;
						}
						value.Add(item: datum as Atom);
					}
				}
			}
			foreach (var cell in staticatoms) {
				foreach (var atom in cell.Value) {
					if (atom.Icon == "" || atom.Icon == null) {
						continue;
					}
					if (atom is not StaticGeom sg) {
						continue;
					}
					if (!materials.TryGetValue(key: atom.Icon, value: out var mat)) {
						var iconpath = $"res://{atom.Icon}";
						Log.Instance.Print(message: $"loading icon '{iconpath}'");
						mat = IconMaterial.FromDMI(path: iconpath);
						materials[key: atom.Icon] = mat;
					}
					if (!surfaces.TryGetValue(key: atom.Icon, value: out var st)) {
						st = new SurfaceTool();
						st.Begin(primitive: Mesh.PrimitiveType.Triangles);
						surfaces.Add(key: atom.Icon, value: st);
					}
					sg.RenderStaticGeom(
						map: grid,
						pos: cell.Key,
						atom: atom,
						st: ref st,
						mat: ref mat
					);
				}
			}
			var finalmesh = new ArrayMesh();
			foreach (var st in surfaces) {
				st.Value.GenerateNormals();
				st.Value.GenerateTangents();
				var mesharr = st.Value.CommitToArrays();
				finalmesh.AddSurfaceFromArrays(
					primitive: Mesh.PrimitiveType.Triangles,
					arrays: mesharr
				);
				finalmesh.SurfaceSetMaterial(
					surfIdx: finalmesh.GetSurfaceCount() - 1,
					material: materials[key: st.Key]
				);
			}
			var mesh = new MeshInstance3D {
				Name = "grid",
				Mesh = finalmesh,
			};
			Log.Instance.Print(message: $"Total Unmapped Atoms: {grid.UnmappedAtoms.Count}");
			Log.Instance.Print(message: $"total Mesh Polys: {mesh.Mesh.GetFaces().Length}");
			Log.Instance.Print(message: $"Total Mesh Surfaces: {surfaces.Count}");
			Log.Instance.Print(message: $"Total Mesh Materials: {materials.Count}");
			return mesh;
		}
		catch (Exception e) {
			Log.Instance.Error(e: e);
			throw;
		}
	}
}