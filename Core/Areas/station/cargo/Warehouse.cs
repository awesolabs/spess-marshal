namespace SpessMarshal.Core.Areas.station.cargo;

public class Warehouse : Cargo {
	public override string DatumPath { get; set; } = "/area/station/cargo/warehouse";

	public override string Name { get; set; } = "Warehouse";
}