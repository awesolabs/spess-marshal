using Godot;
using System.Linq;

[GlobalClass]
public partial class Player : CharacterBody3D, IHealthComponent {
	[Export]
	public float JumpForce = 3.0f;

	[Export]
	public AnimationPlayer Animation;

	[Export]
	public Node3D Model;

	[Export]
	public Camera3D PlayerCamera;

	[Export]
	public Sprite3D PlayerCrosshair;

	[Export]
	public Node3D MovementReference;

	[Export]
	public float MovementSpeed = 2.0f;

	[Export]
	public AudioStreamPlayer3D MovementAudio;

	[Export]
	public float RunSpeedModifier = 1.8f;

	[Export]
	public float Health { get; set; } = 100.0f;

	[Export]
	public float Stamina = 100f;

	[Export]
	public Timer FireTimer;

	[Export]
	public AudioStreamPlayer3D FireAudio;

	public bool Dead;

	private bool MouseFocused;

	public override void _Ready() {
		this.FireTimer.OneShot = true;
		this.FireTimer.Timeout += this.HandleFireTimeout;
		this.FireAudio.VolumeDb = -18;
	}

	public override void _Process(double delta) {
		this.UpdateNeuralWare();
		if (this.Dead) {
			this.FireTimer.Stop();
			Input.MouseMode = Input.MouseModeEnum.Visible;
			return;
		}
		if (this.Health <= 0.0f) {
			this.Animation.Play(name: "CharacterArmature|Death");
			this.Animation.AnimationFinished += (_) => {
				this.Animation.CurrentAnimation = "CharacterArmature|Death";
				this.Animation.Stop(keepState: true);
			};
			this.Dead = true;
			this.GetChildren()
				.OfType<CollisionShape3D>()
				.FirstOrDefault()
				.Disabled = true;
			return;
		}
		if (this.PlayerCamera != null) {
			this.HandleFireTimeout();
			this.HandleMouseMovement();
			this.HandleMovement(delta: delta);
		}
	}

	private void HandleFireTimeout() {
		if (!this.MouseFocused) {
			return;
		}
		if (!Input.IsActionPressed(action: "fire_primary")) {
			return;
		}
		if (this.FireTimer.TimeLeft <= 0) {
			_ = Game.Instance
				.LoadScene<Bullet>(scene: Game.Instance.Bullet)
				.Then(projectile => {
					return Game.Instance.RunMainThread(() => {
						projectile.From = this;
						projectile.Position = this.PlayerCrosshair.GlobalTransform.Origin;
						projectile.Rotation = this.PlayerCamera.GetParent<Node3D>().Rotation;
						projectile.Velocity = -this.PlayerCrosshair.GlobalTransform.Basis.Z * projectile.Speed;
						projectile.InFlight = true;
						this.FireAudio.PitchScale = 1.0f * Game.Instance.Timescale;
						this.FireAudio.Play();
					});
				});
			this.FireTimer.Start();
		}
	}

	private void UpdateNeuralWare() {
		if (this.PlayerCamera == null) {
			return;
		}
		var gridpos = this.GridPos();
		NeuralWare.Instance.GridPosition.Text = $"{gridpos.X}, {-gridpos.Z}";
		var map = Game.Instance.Map;
		if (map.Data.TryGetValue(key: gridpos, value: out var atoms)) {
			var area = atoms.OfType<Area>().FirstOrDefault();
			if (area == null) {
				NeuralWare.Instance.CurrentArea.Text = $"unspecified";
			}
			else {
				NeuralWare.Instance.CurrentArea.Text = !string.IsNullOrWhiteSpace(value: area.Name)
					? area.Name
					: area.DatumPath;
			}
		}
		else {
			NeuralWare.Instance.CurrentArea.Text = "unknown";
		}
		NeuralWare.Instance.Health.Value = this.Health;
		NeuralWare.Instance.Stamina.Value = this.Stamina;
	}

	private void HandleMouseMovement() {
		var camPivot = this.PlayerCamera.GetParent<Node3D>();
		var newPitch = Mathf.Clamp(
			value: (
				camPivot.RotationDegrees.X -
				Game.Instance.MouseDelta.Y *
				Game.Instance.MouseSensitivity
			),
			min: -90,
			max: 90
		);
		var newYaw = (
			camPivot.RotationDegrees.Y -
			Game.Instance.MouseDelta.X *
			Game.Instance.MouseSensitivity
		);
		camPivot.RotationDegrees = new Vector3(
			x: newPitch,
			y: newYaw,
			z: 0
		);
		this.MovementReference.RotationDegrees = new Vector3(
			x: 0,
			y: newYaw,
			z: 0
		);
	}

	private void HandleMovement(double delta) {
		var speed = this.MovementSpeed;
		var direction = new Vector3();
		if (Input.IsMouseButtonPressed(button: MouseButton.Left)) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
			this.MouseFocused = true;
		}
		if (Input.IsKeyPressed(keycode: Key.Escape)) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
			this.MouseFocused = false;
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
		if (Input.IsActionPressed(action: "move_run") && this.Stamina > 0.0f) {
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
		if (this.IsOnFloor() && Input.IsActionPressed(action: "jump")) {
			velocityY += this.JumpForce;
		}
		else {
			var gridpos = this.GridPos();
			var mapCellExists = Game.Instance.Map.Data.TryGetValue(
				key: gridpos,
				value: out var _
			);
			if (mapCellExists) {
				velocityY += this.Velocity.Y + Game.Instance.Gravity * (float)delta;
			}
		}
		this.Velocity = new Vector3(
			x: velocityX,
			y: velocityY,
			z: velocityZ
		);
		this.MoveAndSlide();
		this.UpdateAnimation(speed: speed, delta: delta);
	}

	private void UpdateAnimation(float speed, double delta) {
		if (!this.IsOnFloor()) {
			this.Animation.Play("CharacterArmature|Idle");
			return;
		}
		var runIntent = Input.IsActionPressed(action: "move_run");
		if (this.Velocity.Length() > 0.1) {
			if (runIntent && this.Stamina > 0.0f) {
				this.MovementAudio.PitchScale = (speed / 2.3f - (float)GD.RandRange(0f, 0.6f)) * Game.Instance.Timescale;
				this.Animation.Play("CharacterArmature|Run");
				this.Stamina = Mathf.Clamp(
					value: this.Stamina - 5.0f * (float)delta,
					min: 0.0f, max: 100.0f
				);
			}
			else {
				this.MovementAudio.PitchScale = (speed / 3) * Game.Instance.Timescale;
				this.Animation.SpeedScale = speed;
				this.Animation.Play("CharacterArmature|Walk");
			}
			if (!this.MovementAudio.Playing) {
				this.MovementAudio.Play();
			}
		}
		else {
			this.Animation.Play("CharacterArmature|Idle");
		}
		if (!runIntent || this.Velocity == Vector3.Zero) {
			if (this.Stamina < 100f) {
				this.Stamina = Mathf.Clamp(
					value: this.Stamina + 10.0f * (float)delta,
					min: 0.0f, max: 100.0f
				);
			}
		}
	}
}
