namespace SpessMarshal.Core.Areas.station.command;

public class Teleporter : Command {
	public override string DatumPath { get; set; } = "/area/station/command/teleporter";

	public override string Name { get; set; } = "Teleporter Room";
}