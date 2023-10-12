using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class Bullet : Area3D {
	[Export]
	public float Speed = 8200.0f;

	[Export]
	public double MaxLifetime = 40;

	[Export]
	public double CurrentLifetime = 0;

	[Export]
	public Godot.Collections.Array<AudioStreamPlayer3D> RicAudio;

	[Export]
	public AudioStreamPlayer3D PierceAudio;

	public Random RicRand = new();

	public bool RicPlayed;

	public bool InFlight = false;

	public Vector3 Velocity = Vector3.Zero;

	public Node3D From;

	private bool Destroy;

	public override void _Ready() {
		this.BodyEntered += this.OnBodyEnter;
		Log.Instance.Print(
			message: $"'{this.GetType().FullName}' spawned at '{this.GlobalPosition}'"
		);
	}

	public override void _PhysicsProcess(double delta) {
		if (this.Destroy) {
			this.QueueFree();
			return;
		}
		this.CurrentLifetime += 1.0f * delta;
		if (this.CurrentLifetime >= this.MaxLifetime) {
			Log.Instance.Print(
				message: $"'this.GetType.FullName' destroyed, exceeded lifetime at '{this.GlobalPosition}'"
			);
			this.QueueFree();
		}
		if (!this.InFlight) {
			return;
		}
		this.Position += this.Velocity * (float)delta;
	}

	private void OnBodyEnter(Node3D body) {
		if (body == this.From) {
			Log.Instance.Print(
				message: $"bullet hitting self '{this.From.GetType().FullName}'"
			);
			return;
		}
		if (!this.InFlight) {
			return;
		}
		if (body is IHealthComponent healthComponent) {
			healthComponent.Health -= 10.0f;
		}
		this.GetChildren()
			.OfType<CollisionShape3D>()
			.FirstOrDefault()
			.Disabled = true;

		AudioStreamPlayer3D hitsound = null;
		if (body is StaticBody3D) {
			var playric = this.RicRand.Next(minValue: 0, maxValue: 5) >= 3;
			if (playric) {
				hitsound = this.RicAudio.ElementAt(
					index: this.RicRand.Next(
						minValue: 0,
						maxValue: this.RicAudio.Count - 1
					)
				);
			}
		}
		if (body is Player || body is SWAT) {
			hitsound = this.PierceAudio;
		}
		if (hitsound != null) {
			hitsound.PitchScale = Game.Instance.Timescale < 1.0f ? 0.5f : 1.0f;
			hitsound.Finished += () => {
				Task.Delay(millisecondsDelay: 1000).Then(action: () => {
					this.Destroy = true;
				}).ConfigureAwait(false);
			};
		}
		Game.Instance.RunMainThread(
			action: () => {
				if (hitsound != null) {
					hitsound.Play();
				}
				else {
					this.Destroy = true;
				}
			}
		);
	}
}
