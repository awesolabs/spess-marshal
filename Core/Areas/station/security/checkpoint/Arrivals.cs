namespace SpessMarshal.Core.Areas.station.security.checkpoint;

public class Arrivals : Checkpoint {
	public override string DatumPath { get; set; } = "/area/station/security/checkpoint/arrivals";

	public override string Name { get; set; } = "Arrivals Security Checkpoint";
}