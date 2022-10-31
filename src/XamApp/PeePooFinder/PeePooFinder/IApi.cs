using System.Net.Http;
using System.Threading.Tasks;
using PeePooFinder.Models;
using Refit;

namespace PeePooFinder
{
    public interface IApi
    {
        [Get("/FileManager/BeginFileUpload")]
        Task<string> BeginFileUpload(string fileName);
        [Post("/FileManager/UploadChunk")]
        Task UploadChunk(MediaChunkDto mediaChunk);
        [Get("/FileManager/EndFileUpload")]
        Task EndFileUpload(string fileHandle, bool quitUpload, long fileSize, string OrgFileName);

        [Post("/FileManager")]
        Task UploadFile(MultipartFormDataContent content);
    }
}
