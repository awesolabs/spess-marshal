namespace SpessMarshal.Core.Areas.station.command;

public class Bridge : Command {
	public override string DatumPath { get; set; } = "/area/station/command/bridge";

	public override string Name { get; set; } = "Bridge";
}