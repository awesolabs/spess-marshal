namespace SpessMarshal.Core.Areas.station.security;

public class Brig : Security {
	public override string DatumPath { get; set; } = "/area/station/security/brig";

	public override string Name { get; set; } = "Brig";
}