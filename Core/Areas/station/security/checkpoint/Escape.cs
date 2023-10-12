namespace SpessMarshal.Core.Areas.station.security.checkpoint;

public class Escape : Checkpoint {
	public override string DatumPath { get; set; } = "/area/station/security/checkpoint/escape";

	public override string Name { get; set; } = "Departures Security Checkpoint";
}