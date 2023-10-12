namespace SpessMarshal.Core.Areas.station.security;

public class Warden : Security {
	public override string DatumPath { get; set; } = "/area/station/security/warden";

	public override string Name { get; set; } = "Brig Control";
}