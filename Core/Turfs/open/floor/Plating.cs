namespace SpessMarshal.Core.Turfs.open.floor;

public class Plating : Floor {
	public override string DatumPath { get; set; } = "/turf/open/floor/plating";

	public override string IconState { get; set; } = "plating";

	public override int Dir { get; set; } = 2;
}