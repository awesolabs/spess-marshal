namespace SpessMarshal.Core.Areas.station.security;

public class Interrogation : Security {
	public override string DatumPath { get; set; } = "/area/station/security/interrogation";

	public override string Name { get; set; } = "Interrogation Room";
}