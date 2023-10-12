namespace SpessMarshal.Core.Areas.station.medical;

public class ColdRoom : Medical {
	public override string DatumPath { get; set; } = "/area/station/medical/coldroom";

	public override string Name { get; set; } = "Medical Cold Room";
}