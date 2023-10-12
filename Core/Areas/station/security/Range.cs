namespace SpessMarshal.Core.Areas.station.security;

public class Range : Security {
	public override string DatumPath { get; set; } = "/area/station/security/range";

	public override string Name { get; set; } = "Firing Range";
}