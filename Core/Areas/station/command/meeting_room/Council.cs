namespace SpessMarshal.Core.Areas.station.command.meeting_room;

public class Council : MeetingRoom {
	public override string DatumPath { get; set; } = "/area/station/command/meeting_room/council";

	public override string Name { get; set; } = "Council Chamber";
}