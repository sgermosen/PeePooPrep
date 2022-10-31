using System;
using System.Collections.Generic;
using System.Text;

namespace PeePooFinder.Models
{
    public class MediaChunkDto
    {
        /// <summary>
        /// The unique file handle for this upload.
        /// </summary>
        public string FileHandle { get; set; }

        /// <summary>
        /// The base 64 encoded bytes for this chunk of the file.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The offset of bytes where this chunk starts of the completed file.
        /// </summary>
        public string StartAt { get; set; }
    }
}
