using Godot;

public partial class Menu : Control {
	[Export]
	private Button PlayButton;

	public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Visible;
		this.PlayButton.Pressed += this.OnPlayClicked;
	}

	private void OnPlayClicked() {
		GD.Print(what: "Play clicked");
		Game.Instance.SwitchScene(
			scene: Game.Instance.Act1Scene
		);
	}
}
