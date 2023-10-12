namespace SpessMarshal.Core.Areas.station.cargo;

public class DroneBay : Cargo {
	public override string DatumPath { get; set; } = "/area/station/cargo/drone_bay";

	public override string Name { get; set; } = "Drone Bay";
}