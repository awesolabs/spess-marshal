using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ResourceMap {
	public static readonly ResourceMap Instance = new();

	private readonly Stack<Dictionary<string, Datum>> _overlays = new();

	public void PushOverlay(Dictionary<string, Datum> resources) {
		this._overlays.Push(resources);
	}

	public void PopOverlay() {
		this._overlays.Pop();
	}

	public Dictionary<string, Datum> SearchAssembly(Assembly assembly) {
		return assembly
			.GetTypes()
			.Where(predicate: a => !a.IsAbstract)
			.Where(predicate: a => a.IsAssignableTo(typeof(Datum)))
			.Select(selector: a => Activator.CreateInstance(a) as Datum)
			.Where(predicate: a => a != null && !string.IsNullOrWhiteSpace(a.DatumPath))
			.DistinctBy(a => a.DatumPath)
			.ToDictionary(
				keySelector: a => a.DatumPath,
				elementSelector: a => a
			);
	}

	public bool Has(string path) {
		return this._overlays.Any(a => a.ContainsKey(key: path));
	}

	public Datum Get(string path) {
		return this._overlays
			.FirstOrDefault(a => a.ContainsKey(path))?
			.FirstOrDefault(a => a.Key == path).Value;
	}
}