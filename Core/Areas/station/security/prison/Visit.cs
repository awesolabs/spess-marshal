namespace SpessMarshal.Core.Areas.station.security.prison;

public class Visit : Prison {
	public override string DatumPath { get; set; } = "/area/station/security/prison/visit";

	public override string Name { get; set; } = "Prison Visitation Area";
}