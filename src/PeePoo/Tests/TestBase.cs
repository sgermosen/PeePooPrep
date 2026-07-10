using Application.Core;
using Application.Interfaces;
using Application.Photos;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Tests
{
    public abstract class TestBase : IDisposable
    {
        private readonly SqliteConnection _connection;
        protected readonly DataContext Context;
        protected readonly IMapper Mapper;

        protected TestBase()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new DataContext(options);
            Context.Database.EnsureCreated();

            var configuration = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);
            Mapper = configuration.CreateMapper();
        }

        protected ApplicationUser AddUser(string id, string userName)
        {
            var user = new ApplicationUser
            {
                Id = id,
                UserName = userName,
                NormalizedUserName = userName.ToUpperInvariant(),
                Email = $"{userName}@test.com",
                DisplayName = userName
            };
            Context.Users.Add(user);
            Context.SaveChanges();
            return user;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }

    public class FakeUserAccessor : IUserAccessor
    {
        public string Username { get; set; }
        public string GetUsername() => Username;
    }

    public class FakePhotoAccessor : IPhotoAccessor
    {
        private static PhotoUploadResult Result() => new PhotoUploadResult { PublicId = "fake-id", Url = "https://example.com/photo.jpg" };
        public Task<PhotoUploadResult> AddPhoto(IFormFile file) => Task.FromResult(Result());
        public Task<PhotoUploadResult> AddPhotoLarge(Stream streamdata, string FileName) => Task.FromResult(Result());
        public Task<PhotoUploadResult> AddPhotoLargeFile(IFormFile file) => Task.FromResult(Result());
        public Task<string> DeletePhoto(string publicId) => Task.FromResult("ok");
    }
}
