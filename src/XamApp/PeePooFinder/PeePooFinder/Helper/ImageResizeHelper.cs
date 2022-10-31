
using System;
using System.IO;
using System.Threading.Tasks;


//using System.Drawing;
//using UIKit;
//using CoreGraphics;


using Android.Graphics;


namespace PeePooFinder.Helper
{
    public static class ImageResizeHelper
    {
        static ImageResizeHelper()
        {
        }

        public static async Task<byte[]> ResizeImage(byte[] imageData, float width, float height)
        {
//#if __IOS__
//            return ResizeImageIOS(imageData, width, height);
//#endif
//#if __ANDROID__
			return ResizeImageAndroid ( imageData, width, height );
//#endif
        }


        //public static byte[] ResizeImageIOS(byte[] imageData, float width, float height)
        //{
        //    UIImage originalImage = ImageFromByteArray(imageData);
        //    UIImageOrientation orientation = originalImage.Orientation;

        //    //create a 24bit RGB image
        //    using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
        //                                         (int)width, (int)height, 8,
        //                                         4 * (int)width, CGColorSpace.CreateDeviceRGB(),
        //                                         CGImageAlphaInfo.PremultipliedFirst))
        //    {

        //        RectangleF imageRect = new RectangleF(0, 0, width, height);

        //        // draw the image
        //        context.DrawImage(imageRect, originalImage.CGImage);

        //        UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage(), 0, orientation);

        //        // save the image as a jpeg
        //        return resizedImage.AsJPEG().ToArray();
        //    }
        //}

        //public static UIKit.UIImage ImageFromByteArray(byte[] data)
        //{
        //    if (data == null)
        //    {
        //        return null;
        //    }

        //    UIKit.UIImage image;
        //    try
        //    {
        //        image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Image load failed: " + e.Message);
        //        return null;
        //    }
        //    return image;
        //}

		
		public static byte[] ResizeImageAndroid (byte[] imageData, float width, float height)
		{
			// Load the bitmap
			Bitmap originalImage = BitmapFactory.DecodeByteArray (imageData, 0, imageData.Length);
			Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

			using (MemoryStream ms = new MemoryStream())
			{
				resizedImage.Compress (Bitmap.CompressFormat.Jpeg, 100, ms);
				return ms.ToArray ();
			}
		}


    }
}
