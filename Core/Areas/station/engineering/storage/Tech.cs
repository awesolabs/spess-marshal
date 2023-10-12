namespace SpessMarshal.Core.Areas.station.engineering.storage;

public class Tech : Engineering {
	public override string DatumPath { get; set; } = "/area/station/engineering/storage/tech";

	public override string Name { get; set; } = "Technical Storage";
}