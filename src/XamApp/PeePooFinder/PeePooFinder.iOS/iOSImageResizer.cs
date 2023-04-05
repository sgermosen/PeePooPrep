using CoreGraphics;
using Foundation;
using PeePooFinder.Contracts;
using System;
using UIKit;

namespace PeePooFinder.iOS
{
    public class iOSImageResizer : IImageResizer
    {
        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            UIImage resizedImage = ResizeUIImage(originalImage, width, height);

            using (NSData resizedImageData = resizedImage.AsJPEG())
            {
                byte[] byteArray = new byte[resizedImageData.Length];
                System.Runtime.InteropServices.Marshal.Copy(resizedImageData.Bytes, byteArray, 0, Convert.ToInt32(resizedImageData.Length));
                return byteArray;
            }
        }

        private UIImage ImageFromByteArray(byte[] imageData)
        {
            if (imageData == null)
            {
                return null;
            }

            UIImage image;
            try
            {
                image = new UIImage(NSData.FromArray(imageData));
            }
            catch
            {
                return null;
            }
            return image;
        }

        private UIImage ResizeUIImage(UIImage sourceImage, float width, float height)
        {
            UIGraphics.BeginImageContext(new CGSize(width, height));
            sourceImage.Draw(new CGRect(0, 0, width, height));
            UIImage resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }
    }
}