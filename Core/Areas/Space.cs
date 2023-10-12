namespace SpessMarshal.Core.Areas;

using Core;

public class Space : Area {
	public override string Name { get; set; } = "Space";

	public override string DatumPath { get; set; } = "/area/space";
}