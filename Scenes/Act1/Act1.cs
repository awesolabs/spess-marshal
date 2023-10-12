using System;
using System.Threading.Tasks;
using Godot;

public partial class Act1 : Node3D {
	private BoxTrigger NextScene;

	private string OpeningBriefingDialog = """
	Marshal T.J. Maxx, this is your pre-recorded mission briefing:

	Your primary objective is to kill or capture the Syndicate leader known as, "the viper"

	Intelligence indicates she is harboring on the station known as KiloStation. You are authorized to dispatch any hostile associates Do not let her escape.

	You will be deployed to KiloStation via a corporate sponsored Bluespace Portal.You may utilize station resources to complete your mission but due to management concerns cost of any resources used will be deducted from your mission allowance.

	The Asset and Shrink prevention department has authorized one additional Bluespace portal for your departure. After meeting your key performance indicators you will be awarded the balance of your mission allowance.

	We look forward to your dedication bringing value to our shareholders.
	""";

	public override void _Ready() {
		Task.Run(async () => {
			try {
				var game = Game.Instance;
				var map = await game.LoadMap(
					mapFile: "res://Assets/KiloStation.dmm.txt"
				);
				var trigger = await game.LoadMob<BoxTrigger>(
					scene: game.BoxTrigger
				);
				var agent = await game.LoadMob<Player>(
					scene: game.AgentMob
				);
				trigger.Filter.Add(item: agent);
				await game.MoveMobToRandomSpawn(
					node: agent
				);
				await game.MoveMobToFarthestSpawn(
					node: trigger,
					from: agent.GlobalPosition
				);
				await game.RunMainThread(
					action: () => agent.PlayerCamera?.MakeCurrent()
				);
				for (var a = 0 ; a < 50 ; a++) {
					var swat = await game.LoadMob<SWAT>(scene: game.SwatMob);
					await game.MoveMobToRandomSpawn(
						node: swat,
						exclude: (from: agent, radius: 10.0f)
					);
				}
				await game.RunMainThread(
					action: () => trigger.Area.Monitoring = true
				);
				trigger.OnTriggered += this.OnSceneTrigger;
			}
			catch (Exception e) {
				Log.Instance.Error(e, $"error initializing '{this.GetType().FullName}'");
			}
		});
	}

	private void OnSceneTrigger(Node3D body) {
		Log.Instance.Print(
			message: $"switching scenes"
		);
		Game.Instance.SwitchScene(
			scene: Game.Instance.Act2Scene
		);
	}
}
