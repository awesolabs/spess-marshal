namespace SpessMarshal.Core.Areas.station.ai_monitored.turret_protected;

public class Ai : TurretProtected {
	public override string DatumPath { get; set; } = "/area/station/ai_monitored/turret_protected/ai";

	public override string Name { get; set; } = "AI Chamber";
}