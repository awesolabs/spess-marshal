namespace SpessMarshal.Core.Areas.station.maintenance.disposal;

public class Incinerator : Disposal {
	public override string DatumPath { get; set; } = "/area/station/maintenance/disposal/incinerator";

	public override string Name { get; set; } = "Incinerator";
}