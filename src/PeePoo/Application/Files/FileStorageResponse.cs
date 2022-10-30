using System;

namespace Application.Files
{
    public class FileStorageResponse
    {
        public string Extension { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public byte[] File { get; set; }
        public string Url { get; set; }
        public long Size { get; set; }

    }
}
