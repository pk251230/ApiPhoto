using Api.Services;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;

public class FaceRecognitionService
{
	public async Task<bool> CompareFacesAsync(Stream image1Stream, Stream image2Stream)
	{
		try
		{
			var img1 = ConvertStreamToImage(image1Stream);
			var img2 = ConvertStreamToImage(image2Stream);

			var face1 = DetectFace(img1);
			var face2 = DetectFace(img2);

			if (face1 == null || face2 == null)
			{
				Console.WriteLine("Face not detected in one or both images.");
				return false;
			}

			var resizedFace1 = ResizeImage(face1, 100, 100);
			var resizedFace2 = ResizeImage(face2, 100, 100);

			var gray1 = resizedFace1.Convert<Gray, byte>();
			var gray2 = resizedFace2.Convert<Gray, byte>();

			Mat hist1 = ComputeHistogram(gray1);
			Mat hist2 = ComputeHistogram(gray2);

			hist1.ConvertTo(hist1, DepthType.Cv32F);
			hist2.ConvertTo(hist2, DepthType.Cv32F);

			double similarity = CvInvoke.CompareHist(hist1, hist2, HistogramCompMethod.Correl);

			return similarity > 0.7;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			return false;
		}
	}

	private Mat ComputeHistogram(Image<Gray, byte> image)
	{
		Mat hist = new Mat();

		int[] histSize = { 256 };
		float[] range = { 0, 256 };
		int[] channels = { 0 };

		using (VectorOfMat vm = new VectorOfMat(image.Mat))
		{
			CvInvoke.CalcHist(
				vm,
				channels,
				null,
				hist,
				histSize,
				range,
				false
			);
		}
		return hist;
	}

	private Image<Bgr, byte> ConvertStreamToImage(Stream imageStream)
	{
		using var ms = new MemoryStream();
		imageStream.CopyTo(ms);
		ms.Position = 0;

		// Load the image using ImageSharp
		var imageSharpImage = SixLabors.ImageSharp.Image.Load<Rgb24>(ms);
		var bitmap = imageSharpImage.ToBgrByteImage();

		return bitmap;
	}

	private Image<Bgr, byte>? DetectFace(Image<Bgr, byte> image)
	{
		var grayImage = image.Convert<Gray, byte>();

		// Load multiple face cascades for better detection
		var frontalFaceCascade = new CascadeClassifier("Services/haarcascades/haarcascade_frontalface_alt2.xml");
		var profileFaceCascade = new CascadeClassifier("Services/haarcascades/haarcascade_profileface.xml");

		var faces = frontalFaceCascade.DetectMultiScale(grayImage, 1.1, 12, new System.Drawing.Size(50, 50));

		if (faces.Length == 0)
		{
			faces = profileFaceCascade.DetectMultiScale(grayImage, 1.1, 12, new System.Drawing.Size(50, 50));
		}

		if (faces.Length == 0)
			return null; // No face detected

		// Check for eyes in the detected face region
		var faceROI = image.Copy(faces[0]);
		if (!ContainsEyes(faceROI))
			return null; // Reject if no eyes detected

		return faceROI.Convert<Bgr, byte>(); // Return detected human face
	}

	private bool ContainsEyes(Image<Bgr, byte> faceImage)
	{
		var grayFace = faceImage.Convert<Gray, byte>();
		var eyeCascade = new CascadeClassifier("Services/haarcascades/haarcascade_eye.xml");

		var eyes = eyeCascade.DetectMultiScale(grayFace, scaleFactor: 1.1, minNeighbors: 10, minSize: new System.Drawing.Size(15, 15));

		return eyes.Length >= 1; // At least one eye must be detected
	}

	private Image<Bgr, byte> ResizeImage(Image<Bgr, byte> image, int width, int height)
	{
		var resizedImage = image.Resize(width, height, Inter.Linear);
		return resizedImage;
	}

	public async Task<bool> CheckFacesAsync(Stream image1Stream)
	{
		try
		{
			var img1 = ConvertStreamToImage(image1Stream);

			var face1 = DetectFace(img1);

			if (face1 == null)
			{
				Console.WriteLine("Face not detected in image.");
				return false;
			}
			else
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error: {ex.Message}");
			return false;
		}
	}
}