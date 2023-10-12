using Godot;
using System;

public partial class NeuralWare : Control {
	public static NeuralWare Instance;

	[Export]
	public Label GridPosition;

	[Export]
	public Label CurrentArea;

	[Export]
	public ProgressBar Health;

	[Export]
	public ProgressBar Stamina;

	[Export]
	public ProgressBar SlowMo;

	public override void _Ready() {
		Instance = this;
	}
}
