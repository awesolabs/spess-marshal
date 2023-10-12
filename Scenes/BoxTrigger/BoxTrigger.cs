using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class BoxTrigger : Node3D {

	public Area3D Area;

	public CollisionShape3D Shape;

	public event Action<Node3D> OnTriggered;

	public List<Node3D> Filter = new();

	public override void _Ready() {
		this.Area = this.GetNode<Area3D>("Area3D");
		this.Shape = this.GetNode<CollisionShape3D>("Area3D/CollisionShape3D");
		this.Area.BodyEntered += this.OnBodyEnter;
	}

	private void OnBodyEnter(Node3D body) {
		Log.Instance.Print(
			message: (
				$"Body '{body.Name}' " +
				$"of type '{body.GetType().FullName}' " +
				$"entered at '{this.GlobalPosition}'"
			)
		);
		if (this.Filter.Any()) {
			if (this.Filter.Contains(body)) {
				this.OnTriggered(body);
			}
		}
		else {
			this.OnTriggered(body);
		}
	}
}
