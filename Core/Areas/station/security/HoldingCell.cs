namespace SpessMarshal.Core.Areas.station.security;

public class HoldingCell : Security {
	public override string DatumPath { get; set; } = "/area/station/security/holding_cell";

	public override string Name { get; set; } = "Holding Cell";
}