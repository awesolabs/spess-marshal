namespace SpessMarshal.Core.Areas.station.security;

public class Prison : Security {
	public override string DatumPath { get; set; } = "/area/station/security/prison";

	public override string Name { get; set; } = "Prison Wing";
}