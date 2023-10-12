public class Atom : Datum {
	public override string DatumPath { get; set; } = "/atom";

	[DatumProp(name: "base_icon_state")]
	public virtual string BaseIconState { get; set; } = string.Empty;

	[DatumProp(name: "icon")]
	public virtual string Icon { get; set; } = string.Empty;

	[DatumProp(name: "icon_state")]
	public virtual string IconState { get; set; } = string.Empty;

	[DatumProp(name: "dir")]
	public virtual int Dir { get; set; } = 2;
}