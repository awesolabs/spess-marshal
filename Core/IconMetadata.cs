using System;
using System.Collections.Generic;

public class IconMetadata {
	public int Height;

	public List<State> States = new();

	public string Version = string.Empty;

	public int Width;

	public static IconMetadata FromDMIText(string input) {
		var meta = new IconMetadata();
		var lines = input.Split(separator: '\n');
		foreach (var line in lines) {
			var trimmedLine = line.Trim();
			if (trimmedLine.StartsWith(value: "#")) {
				continue;
			}
			if (trimmedLine.StartsWith(value: "version")) {
				var parts = trimmedLine.Split(separator: '=');
				meta.Version = parts[1].Trim();
				continue;
			}
			if (trimmedLine.StartsWith(value: "width")) {
				var parts = trimmedLine.Split(separator: '=');
				meta.Width = int.Parse(s: parts[1].Trim());
				continue;
			}
			if (trimmedLine.StartsWith(value: "height")) {
				var parts = trimmedLine.Split(separator: '=');
				meta.Height = int.Parse(s: parts[1].Trim());
				continue;
			}
			if (trimmedLine.StartsWith(value: "state")) {
				var state = new State();
				meta.States.Add(item: state);
				var parts = trimmedLine.Split(separator: '=');
				state.Name = parts[1].Trim().Replace(oldValue: "\"", newValue: "");
				continue;
			}
			if (trimmedLine.StartsWith(value: "dirs")) {
				var parts = trimmedLine.Split(separator: '=');
				var dirs = int.Parse(s: parts[1].Trim());
				meta.States[index: meta.States.Count - 1].Dirs = dirs;
				continue;
			}
			if (trimmedLine.StartsWith(value: "frames")) {
				var parts = trimmedLine.Split(separator: '=');
				var frames = int.Parse(s: parts[1].Trim());
				meta.States[index: meta.States.Count - 1].Frames = frames;
				continue;
			}
			if (trimmedLine.StartsWith(value: "delay")) {
				var parts = trimmedLine.Split(separator: '=');
				var delays = parts[1].Trim().Split(separator: ',');
				var delay = Array.ConvertAll(array: delays, converter: float.Parse);
				meta.States[index: meta.States.Count - 1].Delay = delay;
				continue;
			}
			if (trimmedLine.StartsWith(value: "rewind")) {
				var parts = trimmedLine.Split(separator: '=');
				var rewind = int.Parse(s: parts[1].Trim());
				meta.States[index: meta.States.Count - 1].Rewind = rewind;
			}
		}
		return meta;
	}

	public (int X, int Y) GetTextureCoords(
		int texWidth,
		int texHeight,
		string state,
		int dir = 1,
		int frame = 1
	) {
		(int X, int Y) result = new();
		var maxoffsetX = texWidth / this.Width;
		var maxoffsetY = texHeight / this.Height;
		var tileoffset = 0;
		foreach (var currState in this.States) {
			if (currState.Name != state) {
				tileoffset += currState.Frames * currState.Dirs;
				continue;
			}
			//dir = Math.Clamp(dir, 1, currState.Dirs);
			var offset = dir switch {
				10 => 5,
				9 => 7,
				8 => 3,
				7 => 0,
				6 => 4,
				5 => 6,
				4 => 2,
				3 => 0,
				2 => 0,
				1 => 1,
				_ => 0,
			};
			tileoffset += offset * currState.Frames + (frame - 1);
			result.X = tileoffset % maxoffsetX * this.Width;
			result.Y = tileoffset / maxoffsetY * this.Height;
			break;
		}
		return result;
	}

	public enum Directions {
		NORTH = 1,
		SOUTH = 2,
		EAST = 4,
		WEST = 8,
		NORTHWEST = NORTH | WEST,
		NORTHEAST = NORTH | EAST,
		SOUTHEAST = SOUTH | EAST,
		SOUTHWEST = SOUTH | WEST,
	}

	public class State {
		public string Name { get; set; } = string.Empty;

		public int Dirs {
			get;
			set;
		}

		public int Frames {
			get;
			set;
		}

		public float[] Delay { get; set; } = new float[0];

		public int Rewind {
			get;
			set;
		}
	}
}