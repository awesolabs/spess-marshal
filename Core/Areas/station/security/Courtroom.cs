namespace SpessMarshal.Core.Areas.station.security;

public class Courtroom : Security {
	public override string DatumPath { get; set; } = "/area/station/security/courtroom";

	public override string Name { get; set; } = "Courtroom";
}