using System;
using System.Threading.Tasks;
using Godot;

public static class Extensions {
	public static async Task Then(this Task task, Action action) {
		await task;
		action();
	}

	public static async Task Then<T>(this Task<T> task, Action<T> action) {
		var value = await task;
		action(value);
	}

	public static async Task<TV> Then<T, TV>(this Task<T> task, Func<T, TV> then) {
		var result = await task.ConfigureAwait(false);
		return then(result);
	}

	public static Vector3 GridPos(this Node3D node) {
		var pos = new Vector3(
			x: (int)node.GlobalPosition.X,
			y: (int)node.GlobalPosition.Y,
			z: (int)node.GlobalPosition.Z
		);
		return pos;
	}
}