namespace SpessMarshal.Core.Areas.station.security.execution;

public class Transfer : Execution {
	public override string DatumPath { get; set; } = "/area/station/security/execution/transfer";

	public override string Name { get; set; } = "Transfer Centre";
}