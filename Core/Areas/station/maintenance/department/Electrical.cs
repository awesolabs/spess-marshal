namespace SpessMarshal.Core.Areas.station.maintenance.department;

public class Electrical : Department {
	public override string DatumPath { get; set; } = "/area/station/maintenance/department/electrical";

	public override string Name { get; set; } = "Electrical Maintenance";
}