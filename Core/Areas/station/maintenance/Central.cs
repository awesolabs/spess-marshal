namespace SpessMarshal.Core.Areas.station.maintenance;

public class Central : Maintenance {
	public override string DatumPath { get; set; } = "/area/station/maintenance/central";

	public override string Name { get; set; } = "Central Maintenance";
}