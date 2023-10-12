namespace SpessMarshal.Core.Areas.station.maintenance.department.medical;

public class Morgue : Medical {
	public override string DatumPath { get; set; } = "/area/station/maintenance/department/medical/morgue";

	public override string Name { get; set; } = "Morgue Maintenance";
}