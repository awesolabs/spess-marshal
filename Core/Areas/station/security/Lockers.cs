namespace SpessMarshal.Core.Areas.station.security;

public class Lockers : Security {
	public override string DatumPath { get; set; } = "/area/station/security/lockers";

	public override string Name { get; set; } = "Security Locker Room";
}