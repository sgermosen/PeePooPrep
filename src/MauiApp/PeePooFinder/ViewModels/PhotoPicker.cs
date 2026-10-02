namespace PeePooFinder.ViewModels;

/// <summary>Picks or captures one photo and keeps its bytes for upload.</summary>
internal sealed class PhotoPicker
{
    private const long MaxBytes = 10 * 1024 * 1024;
    private readonly IMediaPicker _mediaPicker;

    public PhotoPicker(IMediaPicker mediaPicker)
    {
        _mediaPicker = mediaPicker;
    }

    public byte[]? Bytes { get; private set; }
    public string? FileName { get; private set; }

    public async Task<ImageSource?> PickAsync()
    {
        var photos = await _mediaPicker.PickPhotosAsync(new MediaPickerOptions { SelectionLimit = 1 });
        return await LoadAsync(photos?.FirstOrDefault());
    }

    public async Task<ImageSource?> CaptureAsync()
    {
        if (!_mediaPicker.IsCaptureSupported)
            throw new NotSupportedException("This device has no camera.");
        return await LoadAsync(await _mediaPicker.CapturePhotoAsync());
    }

    public void Clear()
    {
        Bytes = null;
        FileName = null;
    }

    private async Task<ImageSource?> LoadAsync(FileResult? photo)
    {
        if (photo is null) return null;
        await using var stream = await photo.OpenReadAsync();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        if (memory.Length > MaxBytes)
            throw new InvalidOperationException("That photo is larger than 10 MB. Try another one.");

        Bytes = memory.ToArray();
        FileName = photo.FileName;
        var bytes = Bytes;
        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }
}
