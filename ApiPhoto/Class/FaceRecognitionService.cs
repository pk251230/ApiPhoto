
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace ApiPhoto.Class
{
	public class FaceRecognitionService
	{
		//private readonly CascadeClassifier _faceCascade;

		//public FaceRecognitionService()
		//{
			
		//	//_faceCascade = new CascadeClassifier("haarcascades/haarcascade_frontalface_default.xml");
		//	_faceCascade = new CascadeClassifier("haarcascades/haarcascade_frontalface_alt2.xml");
		//}
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

				var resizedFace1 = face1.Resize(100, 100, Inter.Linear);
				var resizedFace2 = face2.Resize(100, 100, Inter.Linear);

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

			using var bitmap = new System.Drawing.Bitmap(ms);
			return bitmap.ToImage<Bgr, byte>(); 
		}
		//private Image<Bgr, byte>? DetectFace(Image<Bgr, byte> image)
		//{
		//	var grayImage = image.Convert<Gray, byte>();

		//	// Strict face detection: Higher minNeighbors, minimum size to avoid false detections
		//	var faces = _faceCascade.DetectMultiScale(grayImage, scaleFactor: 1.1, minNeighbors: 15, minSize: new Size(50, 50));

		//	if (faces.Length == 0)
		//		return null; // No face detected

		//	// Further verify it's a human face by checking for eyes
		//	var faceROI = image.Copy(faces[0]);
		//	if (!ContainsEyes(faceROI))
		//		return null; // Reject if no eyes detected

		//	return faceROI.Convert<Bgr, byte>(); // Return the detected human face
		//}
		private Image<Bgr, byte>? DetectFace(Image<Bgr, byte> image)
		{
			var grayImage = image.Convert<Gray, byte>();

			// Load multiple face cascades for better detection
			var frontalFaceCascade = new CascadeClassifier("haarcascades/haarcascade_frontalface_alt2.xml");
			var profileFaceCascade = new CascadeClassifier("haarcascades/haarcascade_profileface.xml");

			var faces = frontalFaceCascade.DetectMultiScale(grayImage, 1.1, 12, new Size(50, 50));

			if (faces.Length == 0)
			{
				faces = profileFaceCascade.DetectMultiScale(grayImage, 1.1, 12, new Size(50, 50));
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
			var eyeCascade = new CascadeClassifier("haarcascades/haarcascade_eye.xml");

			var eyes = eyeCascade.DetectMultiScale(grayFace, scaleFactor: 1.1, minNeighbors: 10, minSize: new Size(15, 15));

			return eyes.Length >= 1; // At least one eye must be detected
		}
		public async Task<bool> CheckFacesAsync(Stream image1Stream)
		{
			try
			{
				var img1 = ConvertStreamToImage(image1Stream);

				var face1 = DetectFace(img1);

				if (face1 == null)
				{
					Console.WriteLine("Face not detected images.");
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
}


