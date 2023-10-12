namespace SpessMarshal.Core.Areas.station.maintenance.port;

public class Aft : Port {
	public override string DatumPath { get; set; } = "/area/station/maintenance/port/aft";

	public override string Name { get; set; } = "Aft Port Maintenance";
}