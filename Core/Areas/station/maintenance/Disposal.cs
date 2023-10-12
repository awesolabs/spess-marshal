namespace SpessMarshal.Core.Areas.station.maintenance;

public class Disposal : Maintenance {
	public override string DatumPath { get; set; } = "/area/station/maintenance/disposal";

	public override string Name { get; set; } = "Waste Disposal";
}