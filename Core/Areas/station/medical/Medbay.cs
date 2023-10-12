namespace SpessMarshal.Core.Areas.station.medical;

public class Medbay : Medical {
	public override string DatumPath { get; set; } = "/area/station/medical/medbay";

	public override string Name { get; set; } = "Medical";
}