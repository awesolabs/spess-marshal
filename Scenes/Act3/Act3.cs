using Godot;
using System;
using System.Threading.Tasks;

public partial class Act3 : Node3D {
	public override void _Ready() {
		Task.Run(async () => {
			try {
				var game = Game.Instance;
				var map = await game.LoadMap(
					mapFile: "res://Assets/DeltaStation2.dmm.txt"
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
					action: () => trigger.Area.Monitoring = true
				);
				await game.RunMainThread(
					action: () => agent.PlayerCamera?.MakeCurrent()
				);
				trigger.OnTriggered += this.OnSceneTrigger;
				for (var a = 0 ; a < 200 ; a++) {
					var swat = await game.LoadMob<SWAT>(scene: game.SwatMob);
					await game.MoveMobToRandomSpawn(
						node: swat,
						exclude: (from: agent, radius: 10.0f)
					);
				}
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
			scene: Game.Instance.CreditsScene
		);
	}
}
