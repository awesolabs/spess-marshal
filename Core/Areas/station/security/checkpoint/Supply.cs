namespace SpessMarshal.Core.Areas.station.security.checkpoint;

public class Supply : Checkpoint {
	public override string DatumPath { get; set; } = "/area/station/security/checkpoint/supply";

	public override string Name { get; set; } = "Security Post - Cargo Bay";
}