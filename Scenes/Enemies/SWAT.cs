using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class SWAT : CharacterBody3D, IHealthComponent {
	[Export]
	public float JumpForce = 3.0f;

	[Export]
	public AudioStreamPlayer3D Audio;

	[Export]
	public AnimationPlayer Animation;

	[Export]
	public Node3D Model;

	[Export]
	public Node3D MovementReference;

	[Export]
	public RayCast3D LookScanRay;

	[Export]
	public float MovementSpeed = 2.0f;

	[Export]
	public float RunSpeedModifier = 1.7f;

	[Export]
	public float Health { get; set; } = 100.0f;

	[Export]
	public Timer FireTimer;

	[Export]
	public AudioStreamPlayer3D FireAudio;

	public bool Dead;

	private bool TrackingTarget;

	private Node3D Target;

	public override void _Ready() {
		this.FireTimer.Timeout += this.HandleFireTimeout;
		this.FireAudio.VolumeDb = -18;
	}

	public override void _Process(double delta) {
		if (this.Dead) {
			this.FireTimer.Stop();
			this.TrackingTarget = false;
			this.Target = null;
			return;
		}
		if (this.Health <= 0.0f) {
			this.Animation.Play(
				name: "CharacterArmature|Death"
			);
			this.Animation.AnimationFinished += (_) => {
				this.Animation.CurrentAnimation = "CharacterArmature|Death";
				this.Animation.Stop(keepState: true);
			};
			this.Dead = true;
			this.GetChildren()
				.OfType<CollisionShape3D>()
				.FirstOrDefault()
				.Disabled = true;
			this.GetChildren()
				.OfType<RayCast3D>()
				.FirstOrDefault()
				.Enabled = false;
		}
	}

	public override void _PhysicsProcess(double delta) {
		if (this.Dead) {
			return;
		}
		this.HandleLookScan(delta: delta);
	}

	public void HandleLookScan(double delta) {
		DebugDraw3D.DrawLine(
			a: this.LookScanRay.GlobalPosition,
			b: (
				this.LookScanRay.GlobalPosition +
				this.LookScanRay.GlobalTransform.Basis.Z
			),
			color: this.TrackingTarget ? Colors.Red : Colors.Green
		);
		var collider = this.LookScanRay.GetCollider();
		switch (collider) {
			case Player player:
				this.TrackingTarget = true;
				this.Target = player;
				break;
			case Area3D a:
				if (a.GetParent() is Player p) {
					this.TrackingTarget = true;
					this.Target = p;
				}
				break;
			default:
				this.TrackingTarget = false;
				this.Target = null;
				break;
		}
		if (this.TrackingTarget) {
			var directionFlat = (
				this.Target.GlobalPosition -
				this.LookScanRay.GlobalPosition
			);
			directionFlat.Y = 0;
			var angle = Mathf.Atan2(
				y: -directionFlat.X,
				x: -directionFlat.Z
			);
			this.LookScanRay.Rotation = new Vector3(
				x: 0,
				y: angle,
				z: 0
			);
			this.Model.Rotation = new Vector3(
				x: 0,
				y: angle,
				z: 0
			);
			this.Animation.Play(name: "CharacterArmature|Gun_Shoot");
		}
		else {
			this.Animation.Play(name: "CharacterArmature|Idle");
			var newYRotation = (float)(
				this.LookScanRay.Rotation.Y +
				Mathf.DegToRad(deg: new Random().Next(220, 480) * delta)
			);
			this.LookScanRay.Rotation = new Vector3(
				x: 0,
				y: newYRotation,
				z: 0
			);
		}
	}

	private void HandleFireTimeout() {
		this.FireTimer.Start();
		if (!this.TrackingTarget) {
			return;
		}
		_ = Game.Instance.LoadScene<Bullet>(
			scene: Game.Instance.Bullet
		).Then(projectile => {
			return Game.Instance.RunMainThread(() => {
				projectile.From = this;
				projectile.Position = this.LookScanRay.GlobalPosition;
				projectile.Rotation = this.LookScanRay.Rotation;
				projectile.Velocity = (
					this.LookScanRay.GlobalTransform.Basis.Z *
					40
				);
				projectile.InFlight = true;
				this.FireAudio.PitchScale = 1.0f * Game.Instance.Timescale;
				this.FireAudio.Play();
			});
		});
	}

	private void HandleMovement(double delta) {
		var speed = this.MovementSpeed;
		var direction = new Vector3();
		if (Input.IsMouseButtonPressed(button: MouseButton.Left)) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		if (Input.IsKeyPressed(keycode: Key.Escape)) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if (Input.IsActionPressed(action: "move_forward")) {
			direction += -this.MovementReference.Transform.Basis.Z;
		}
		if (Input.IsActionPressed(action: "move_backward")) {
			direction -= -this.MovementReference.Transform.Basis.Z;
		}
		if (Input.IsActionPressed(action: "move_left")) {
			direction -= this.MovementReference.Transform.Basis.X;
		}
		if (Input.IsActionPressed(action: "move_right")) {
			direction += this.MovementReference.Transform.Basis.X;
		}
		if (Input.IsActionPressed(action: "move_run")) {
			speed *= this.RunSpeedModifier;
		}
		direction = direction.Normalized();
		if (direction != Vector3.Zero) {
			this.Model.LookAt(
				target: this.GlobalTransform.Origin - direction,
				up: Vector3.Up
			);
		}
		var velocityX = direction.X * speed;
		var velocityZ = direction.Z * speed;
		var velocityY = 0f;
		var mapCellExists = Game.Instance.Map.Data.TryGetValue(
			key: this.GridPos(),
			value: out var _
		);
		if (mapCellExists) {
			velocityY += this.Velocity.Y + Game.Instance.Gravity * (float)delta;
		}
		this.Velocity = new Vector3(
			x: velocityX,
			y: velocityY,
			z: velocityZ
		);
		this.MoveAndSlide();
		this.UpdateAnimation(speed: speed);
	}

	private void UpdateAnimation(float speed) {
		if (!this.IsOnFloor()) {
			this.Animation.Play("CharacterArmature|Idle");
			return;
		}
		if (this.Velocity.Length() > 0.1) {
			if (Input.IsActionPressed(action: "move_run")) {
				this.Audio.PitchScale = (speed / 3 - (float)GD.RandRange(0f, 0.5f)) * Game.Instance.Timescale;
				this.Animation.Play("CharacterArmature|Run");
			}
			else {
				this.Audio.PitchScale = (speed / 3) * Game.Instance.Timescale;
				this.Animation.SpeedScale = speed;
				this.Animation.Play("CharacterArmature|Walk");
			}
			if (!this.Audio.Playing) {
				this.Audio.Play();
			}
		}
		else {
			this.Animation.Play("CharacterArmature|Idle");
		}
	}
}
