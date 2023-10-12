namespace SpessMarshal.Core.Areas.station.service.hydroponics;

public class Garden : Hydroponics {
	public override string DatumPath { get; set; } = "/area/station/service/hydroponics/garden";

	public override string Name { get; set; } = "Garden";
}