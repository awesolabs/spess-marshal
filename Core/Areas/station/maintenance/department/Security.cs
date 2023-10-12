namespace SpessMarshal.Core.Areas.station.maintenance.department;

public class Security : Department {
	public override string DatumPath { get; set; } = "/area/station/maintenance/department/security";

	public override string Name { get; set; } = "Security Maintenance";
}