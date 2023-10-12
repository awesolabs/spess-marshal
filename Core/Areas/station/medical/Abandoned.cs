namespace SpessMarshal.Core.Areas.station.medical;

public class Abandoned : Medical {
	public override string DatumPath { get; set; } = "/area/station/medical/abandoned";

	public override string Name { get; set; } = "Abandoned Medbay";
}