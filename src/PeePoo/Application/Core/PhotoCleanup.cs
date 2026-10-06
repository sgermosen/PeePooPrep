using Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Core
{
    public static class PhotoCleanup
    {
        /// <summary>Removes stored image files after their rows are gone. Failures are logged, never thrown.</summary>
        public static async Task DeleteAsync(IPhotoAccessor accessor, IEnumerable<string> ids, ILogger logger)
        {
            foreach (var id in ids)
            {
                try
                {
                    await accessor.DeletePhoto(id);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Could not delete stored photo {PhotoId}", id);
                }
            }
        }
    }
}
