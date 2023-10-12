using System;
using System.Linq;
using Godot;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image;

public partial class IconMaterial : StandardMaterial3D {
	[Export]
	public Texture Texture;

	public IconMetadata Metadata { get; private set; }

	public static IconMaterial FromDMI(string path) {
		var imageFile = FileAccess.Open(
			path: path,
			flags: FileAccess.ModeFlags.Read
		);
		if (imageFile == null && FileAccess.GetOpenError() is Error oe) {
			var msg = $"Error opening image file: {oe}";
			var ex = new Exception(msg);
			Log.Instance.Error(ex);
			throw ex;
		}
		var imageBytes = imageFile.GetBuffer(
			length: (long)imageFile.GetLength()
		);
		var imageInfo = Image.Load(
			data: imageBytes
		);
		var imageMeta = imageInfo.Metadata.GetFormatMetadata(
			key: PngFormat.Instance
		).TextData.First();
		var im = new IconMaterial();
		var image = new Godot.Image();
		var texture = new ImageTexture();
		image.LoadPngFromBuffer(buffer: imageBytes);
		image.GenerateMipmaps();
		texture.SetImage(image: image);
		//im.TextureFilter = TextureFilterEnum.NearestWithMipmaps;      
		//im.TextureFilter = TextureFilterEnum.Nearest;      
		im.TextureFilter = TextureFilterEnum.NearestWithMipmapsAnisotropic;
		im.AlbedoColor = new Color(code: "F2F3F4");
		im.AlbedoTexture = texture;
		//im.ShadingMode = ShadingModeEnum.Unshaded;
		im.Metadata = IconMetadata.FromDMIText(input: imageMeta.Value);
		return im;
	}

	public Vector2 GetUVCoord(
		string state,
		Vector2 vertcoord,
		ref Vector2 uvcoord,
		int dir = 1
	) {
		var texw = this.AlbedoTexture.GetWidth();
		var texh = this.AlbedoTexture.GetHeight();
		var texcoords = this.Metadata.GetTextureCoords(
			texWidth: texw,
			texHeight: texh,
			state: state,
			dir: dir
		);
		// basis values used to shift x or y uv values based on the position of 
		// the supplied vertex coords against the size of the icon mat tile. 
		// ie: 32px tile / 320px texture = 0.1 basis step size. 
		var basisx = (float)this.Metadata.Width / texw;
		var basisy = (float)this.Metadata.Height / texh;
		// normalize the actual texture coordinates to uv
		// ie: tile at (x: 64px, y: 0px) = 0.2
		var uvx = (float)texcoords.X / texw;
		var uvy = (float)texcoords.Y / texh;
		if (vertcoord == new Vector2(x: 0, y: 0)) {
			uvcoord.X = uvx;
			uvcoord.Y = uvy;
		}
		else if (vertcoord == new Vector2(x: 0, y: 1)) {
			uvcoord.X = uvx;
			uvcoord.Y = uvy + basisy;
		}
		else if (vertcoord == new Vector2(x: 1, y: 0)) {
			uvcoord.X = uvx + basisx;
			uvcoord.Y = uvy;
		}
		else if (vertcoord == new Vector2(x: 1, y: 1)) {
			uvcoord.X = uvx + basisx;
			uvcoord.Y = uvy + basisy;
		}

		return uvcoord;
	}
}