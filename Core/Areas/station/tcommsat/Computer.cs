namespace SpessMarshal.Core.Areas.station.tcommsat;

public class Computer : TCommsat {
	public override string DatumPath { get; set; } = "/area/station/tcommsat/computer";

	public override string Name { get; set; } = "Telecomms Control Room";
}