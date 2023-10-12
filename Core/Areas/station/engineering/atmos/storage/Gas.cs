namespace SpessMarshal.Core.Areas.station.engineering.atmos.storage;

public class Gas : Storage {
	public override string DatumPath { get; set; } = "/area/station/engineering/atmos/storage/gas";

	public override string Name { get; set; } = "Atmospherics Gas Storage";
}