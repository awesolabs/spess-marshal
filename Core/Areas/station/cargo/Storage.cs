namespace SpessMarshal.Core.Areas.station.cargo;

public class Storage : Cargo {
	public override string DatumPath { get; set; } = "/area/station/cargo/storage";

	public override string Name { get; set; } = "Cargo Bay";
}