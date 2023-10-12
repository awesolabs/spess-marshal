namespace SpessMarshal.Core.Areas.station.command;

public class Gateway : Command {
	public override string DatumPath { get; set; } = "/area/station/command/gateway";

	public override string Name { get; set; } = "Gateway";
}