namespace SpessMarshal.Core.Areas.station.cargo;

public class Office : Cargo {
	public override string DatumPath { get; set; } = "/area/station/cargo/office";

	public override string Name { get; set; } = "Cargo Office";
}