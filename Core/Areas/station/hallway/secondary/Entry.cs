namespace SpessMarshal.Core.Areas.station.hallway.secondary;

public class Entry : Secondary {
	public override string Name { get; set; } = "Arrival Shuttle Hallway";

	public override string DatumPath { get; set; } = "/area/station/hallway/secondary/entry";

	public override string Icon { get; set; } = "icons/area/areas_station.png";

	public override string IconState { get; set; } = "entry";
}