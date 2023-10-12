namespace SpessMarshal.Core.Areas.station.security.checkpoint;

public class Engineering : Checkpoint {
	public override string DatumPath { get; set; } = "/area/station/security/checkpoint/engineering";

	public override string Name { get; set; } = "Security Post - Engineering";
}