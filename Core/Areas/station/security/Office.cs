namespace SpessMarshal.Core.Areas.station.security;

public class Office : Security {
	public override string DatumPath { get; set; } = "/area/station/security/office";

	public override string Name { get; set; } = "Security Office";
}