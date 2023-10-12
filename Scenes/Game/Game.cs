using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;

public partial class Game : Node {
	public static Game Instance;

	[Export]
	public float MouseSensitivity = 0.1f;

	[Export]
	public PackedScene MenuScene;

	[Export]
	public PackedScene Act1Scene;

	[Export]
	public PackedScene Act2Scene;

	[Export]
	public PackedScene Act3Scene;

	[Export]
	public PackedScene CreditsScene;

	[Export]
	public PackedScene AgentMob;

	[Export]
	public PackedScene SwatMob;

	[Export]
	public PackedScene BoxTrigger;

	[Export]
	public PackedScene Bullet;

	public Map Map;

	public List<Node3D> Mobs = new();

	private readonly ConcurrentQueue<Action> MainThread = new();

	public float Timescale = 1.0f;

	public Vector2 MouseDelta;

	public float Gravity = -9.8f;

	public float SlowMo = 100f;

	public override void _Ready() {
		Instance = this;
		ResourceMap.Instance.PushOverlay(
			resources: ResourceMap.Instance.SearchAssembly(
				assembly: Assembly.GetExecutingAssembly()
			)
		);
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion mm) {
			this.MouseDelta = mm.Relative;
		}
		if (@event is InputEventKey ek && ek.IsReleased()) {
			if (ek.Keycode == Key.F1) {
				GD.Print(what: "Menu Scene");
				this.SwitchScene(scene: this.MenuScene);
			}
			if (ek.Keycode == Key.F2) {
				GD.Print(what: "Act 1 Scene");
				this.SwitchScene(scene: this.Act1Scene);
			}
			if (ek.Keycode == Key.F3) {
				GD.Print(what: "Act 2 Scene");
				this.SwitchScene(scene: this.Act2Scene);
			}
			if (ek.Keycode == Key.F4) {
				GD.Print(what: "Act 3 Scene");
				this.SwitchScene(scene: this.Act3Scene);
			}
			if (ek.Keycode == Key.F5) {
				GD.Print(what: "Credits Scene");
				this.SwitchScene(scene: this.CreditsScene);
			}
			if (ek.Keycode == Key.T) {
				this.Timescale = this.Timescale < 1.0f ? 1.0f : 0.2f;
				Engine.TimeScale = this.Timescale;
			}
		}
	}

	public override void _Process(double delta) {
		var start = DateTime.UtcNow.Millisecond;
		while (this.MainThread.TryDequeue(result: out var action)) {
			action();
			var timesink = DateTime.UtcNow.Millisecond - start;
			if (timesink >= 16.666) {
				break;
			}
		}
		this.CallDeferred(method: nameof(this.EndFrame));
	}

	private void EndFrame() {
		this.MouseDelta = Vector2.Zero;
	}

	public Task RunMainThread(Action action) {
		var done = new TaskCompletionSource();
		this.MainThread.Enqueue(item: () => {
			action();
			done.SetResult();
		});
		return done.Task;
	}

	public Task<T> RunMainThread<T>(Func<T> func) {
		var done = new TaskCompletionSource<T>();
		this.MainThread.Enqueue(item: () => {
			var result = func();
			done.SetResult(result);
		});
		return done.Task;
	}

	public void SwitchScene(PackedScene scene) {
		this.RunMainThread(() => {
			this.Mobs.Clear();
			this.GetTree().ChangeSceneToPacked(packedScene: scene);
		});
	}

	public Task<T> LoadMob<T>(PackedScene scene) where T : Node3D {
		return Task.Run(async () => {
			return await this.RunMainThread(() => {
				var instance = scene.Instantiate<T>();
				this.GetTree().CurrentScene.AddChild(node: instance);
				this.Mobs.Add(item: instance);
				return instance;
			});
		});
	}

	public Task<T> LoadScene<T>(PackedScene scene) where T : Node {
		return Task.Run(async () => {
			return await this.RunMainThread(() => {
				var instance = scene.Instantiate<T>();
				this.GetTree().CurrentScene.AddChild(node: instance);
				return instance;
			});
		});
	}

	public Task<Map> LoadMap(string mapFile) {
		return Task.Run(async () => {
			var map = Map.FromDMM(path: mapFile, ignore: new());
			map.ApplyMappingMutations();
			var mesh = map.BuildMesh();
			mesh.CreateTrimeshCollision();
			map.AddChild(mesh);
			await this.RunMainThread(action: () => {
				this.GetTree().CurrentScene.AddChild(node: map);
				this.Map = map;
			});
			return map;
		});
	}

	public Task MoveMobToRandomSpawn(Node3D node, (Node3D from, float radius) exclude = default) {
		return Task.Run(async () => {
			var tries = 10;
			while (tries > 0) {
				tries--;
				var random = new Random();
				var spawns = this.Map.Data.Where(
					predicate: (kv) => kv.Value.OfType<Spawn>().Any()
				).Distinct();
				var selectedSpawn = spawns.ElementAt(
					index: random.Next(
						minValue: 0,
						maxValue: spawns.Count()
					)
				);
				var existsAtSpawn = await this.RunMainThread(() => {
					return this.Mobs.Any(
						predicate: m => m.GridPos() == selectedSpawn.Key
					);
				});
				if (existsAtSpawn) {
					Log.Instance.Print(
						message: (
							$"Conflict moving mob to spawn '{selectedSpawn.Key}' " +
							$"mob already exists at location tries left '{tries}'"
						)
					);
					continue;
				}
				if (exclude.from != null && exclude.radius > 0) {
					var dist = await this.RunMainThread(() => {
						return selectedSpawn.Key - exclude.from.GlobalPosition;
					});
					if (dist.Length() <= exclude.radius) {
						Log.Instance.Print(
							message: (
								$"Spawn location at '{selectedSpawn.Key}' " +
								$"within exclude radius '{exclude.radius}' " +
								$"from '{exclude.from}'"
							)
						);
						continue;
					}
				}
				await this.RunMainThread(() => {
					Log.Instance.Print(
						message: (
							$"Moving Node '{node.Name}' " +
							$"of type '{node.GetType().FullName}' " +
							$"to spawn '{selectedSpawn.Key}'"
						)
					);
					node.GlobalPosition = (
						selectedSpawn.Key +
						(Vector3.Forward / 2) +
						(Vector3.Right / 2)
					);
				});
				return;
			}
			Log.Instance.Print(
				message: (
					$"failed to move node '{node.GetType().FullName}' " +
					$"to random spawn location after 10 tries"
				)
			);
		});
	}

	public Task MoveMobToFarthestSpawn(Node3D node, Vector3 from) {
		return Task.Run(async () => {
			var spawns = this.Map.Data.Where(
				predicate: (kv) => kv.Value.OfType<Spawn>().Any()
			);
			var farthest = 0.0f;
			var moveto = Vector3.Zero;
			foreach (var spawn in spawns) {
				var distance = from.DistanceTo(to: spawn.Key);
				if (distance > farthest) {
					farthest = distance;
					moveto = spawn.Key;
				}
			}
			await this.RunMainThread(() => {
				Log.Instance.Print(
					message: (
						$"Moving Node '{node.Name}' " +
						$"of type '{node.GetType().FullName}' " +
						$"to farthest spawn '{farthest}'"
					)
				);
				node.GlobalPosition = (
					moveto +
					(Vector3.Forward / 2) +
					(Vector3.Right / 2)
				);
			});
		});
	}
}
