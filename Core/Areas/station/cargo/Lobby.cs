namespace SpessMarshal.Core.Areas.station.cargo;

public class Lobby : Cargo {
	public override string DatumPath { get; set; } = "/area/station/cargo/lobby";

	public override string Name { get; set; } = "Cargo Lobby";
}