namespace SpessMarshal.Core.Areas.station.medical;

public class Paramedic : Medical {
	public override string DatumPath { get; set; } = "/area/station/medical/paramedic";

	public override string Name { get; set; } = "Paramedic Dispatch";
}