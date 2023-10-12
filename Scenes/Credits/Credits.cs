using Godot;
using System;

public partial class Credits : Control {
	[Export]
	private Button QuitButton;

	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Visible;
		this.QuitButton.Pressed += this.OnQuitPressed;
	}

	private void OnQuitPressed() {
		GD.Print(what: "Quitting Game");
		this.GetTree().Quit();
	}
}
