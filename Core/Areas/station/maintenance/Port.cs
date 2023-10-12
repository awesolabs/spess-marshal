namespace SpessMarshal.Core.Areas.station.maintenance;

public class Port : Maintenance {
	public override string Name { get; set; } = "Port Maintenance";

	public override string DatumPath { get; set; } = "/area/station/maintenance/port";

	public override string Icon { get; set; } = "icons/area/areas_station.png";

	public override string IconState { get; set; } = "portmaint";
}