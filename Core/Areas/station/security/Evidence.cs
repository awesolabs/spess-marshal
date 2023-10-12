namespace SpessMarshal.Core.Areas.station.security;

public class Evidence : Security {
	public override string DatumPath { get; set; } = "/area/station/security/evidence";

	public override string Name { get; set; } = "Evidence Storage";
}