namespace SpessMarshal.Core.Areas.station.medical;

public class Morgue : Medical {
	public override string DatumPath { get; set; } = "/area/station/medical/morgue";

	public override string Name { get; set; } = "Morgue";
}