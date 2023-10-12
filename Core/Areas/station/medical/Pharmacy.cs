namespace SpessMarshal.Core.Areas.station.medical;

public class Pharmacy : Medical {
	public override string DatumPath { get; set; } = "/area/station/medical/pharmacy";

	public override string Name { get; set; } = "Pharmacy";
}