using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

[AttributeUsage(AttributeTargets.Property)]
public class DatumPropAttribute : Attribute {
	public readonly string Name;

	public DatumPropAttribute(string name) {
		this.Name = name;
	}
}

public class Datum {
	[DatumProp(name: "datum_path")]
	public virtual string DatumPath { get; set; } = "/datum";

	[DatumProp(name: "name")]
	public virtual string Name { get; set; } = string.Empty;

	public virtual IEnumerable<Datum> Contents { get; set; } = new List<Datum>();

	public virtual Datum Parent { get; protected set; }

	public void SetProp(string name, object value) {
		var propertiesWithAttributes = this.GetType()
			.GetProperties(
				bindingAttr: BindingFlags.Public | BindingFlags.Instance
			)
			.Where(predicate: p =>
				p.GetCustomAttribute<DatumPropAttribute>() != null &&
				p.GetCustomAttribute<DatumPropAttribute>().Name == name
			);
		foreach (var prop in propertiesWithAttributes) {
			var validValue = value == null || prop.PropertyType.IsAssignableFrom(
				c: value.GetType()
			);
			if (validValue) {
				prop.SetValue(obj: this, value: value);
			}
			else {
				throw new InvalidOperationException(
					message: new string[]{
						$"Type mismatch. Cannot set value of type {value.GetType().Name}",
						$"to property {prop.Name} of type {prop.PropertyType.Name}."
					}.Join(delimiter: " ")
				);
			}
		}
	}
}