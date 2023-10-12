namespace SpessMarshal.Core.Areas.station.ai_monitored.command;

public class NukeStorage : Command {
	public override string DatumPath { get; set; } = "/area/station/ai_monitored/command/nuke_storage";

	public override string Name { get; set; } = "Vault";
}