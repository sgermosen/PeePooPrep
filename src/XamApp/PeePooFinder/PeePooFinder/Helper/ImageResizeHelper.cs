using PeePooFinder.Contracts;
using System.Threading.Tasks;
using Xamarin.Forms;

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
            return ResizeImagePlatform(imageData, width, height);
            //#endif
        }

        public static byte[] ResizeImagePlatform(byte[] imageData, float width, float height)
        {
            return DependencyService.Get<IImageResizer>().ResizeImage(imageData, width, height);
        }

    }
}
