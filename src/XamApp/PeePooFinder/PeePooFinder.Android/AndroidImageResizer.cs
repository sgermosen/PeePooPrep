using Android.Graphics;
using PeePooFinder.Contracts;
using System.IO;

namespace PeePooFinder.Droid
{
    public class AndroidImageResizer : IImageResizer
    {
        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, false);

            using MemoryStream ms = new MemoryStream();
            resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
            return ms.ToArray();
        }
    }
}