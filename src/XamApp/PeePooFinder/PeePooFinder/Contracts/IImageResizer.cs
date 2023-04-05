namespace PeePooFinder.Contracts
{
    public interface IImageResizer
    {
        byte[] ResizeImage(byte[] imageData, float width, float height);
    }

}
