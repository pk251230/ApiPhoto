using Emgu.CV.Structure;
using Emgu.CV;
using SixLabors.ImageSharp.PixelFormats;

namespace Api.Services
{
	public static class ImageSharpExtensions
	{
		public static Image<Bgr, byte> ToBgrByteImage(this SixLabors.ImageSharp.Image<Rgb24> image)
		{
			var width = image.Width;
			var height = image.Height;
			var cvImage = new Image<Bgr, byte>(width, height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var pixel = image[x, y];
					cvImage[y, x] = new Bgr(pixel.B, pixel.G, pixel.R);
				}
			}

			return cvImage;
		}
	}
}
