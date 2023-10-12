namespace SpessMarshal.Core.Areas.station.security;

public class Medical : Security {
	public override string DatumPath { get; set; } = "/area/station/security/medical";

	public override string Name { get; set; } = "Security Medical";
}