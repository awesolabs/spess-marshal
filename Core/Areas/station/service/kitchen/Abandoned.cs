namespace SpessMarshal.Core.Areas.station.service.kitchen;

public class Abandoned : Kitchen {
	public override string DatumPath { get; set; } = "/area/station/service/kitchen/abandoned";

	public override string Name { get; set; } = "Abandoned Kitchen";
}