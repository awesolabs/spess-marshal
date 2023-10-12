namespace SpessMarshal.Core.Areas.station.medical.surgery;

public class Theatre : Surgery {
	public override string DatumPath { get; set; } = "/area/station/medical/surgery/theatre";

	public override string Name { get; set; } = "Grand Surgery Theatre";
}