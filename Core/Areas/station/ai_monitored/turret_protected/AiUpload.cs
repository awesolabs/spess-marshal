namespace SpessMarshal.Core.Areas.station.ai_monitored.turret_protected;

public class AiUpload : TurretProtected {
	public override string DatumPath { get; set; } = "/area/station/ai_monitored/turret_protected/ai_upload";

	public override string Name { get; set; } = "AI Upload Chamber";
}