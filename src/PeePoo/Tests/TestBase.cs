using Application.Core;
using Application.Interfaces;
using Application.Photos;
using Application.Places;
using Application.Visits;
using System.Threading;
using Xunit;
using AutoMapper;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Persistence;
using System;
using System.Collections.Generic;
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

        protected static FakeUserAccessor As(string username, bool admin = false) =>
            new FakeUserAccessor { Username = username, Admin = admin };

        protected static PlaceInput NewPlace(string name = "Cafe Central WC", double lat = 18.4861, double lng = -69.9312) => new PlaceInput
        {
            Name = name,
            Description = "Bright and clean",
            Type = "Unisex",
            Rating = 4,
            Lat = lat,
            Long = lng
        };

        protected async Task<Guid> CreatePlaceAs(string username, PlaceInput input = null, FakePhotoAccessor photos = null)
        {
            input ??= NewPlace();
            var result = await new Application.Places.Create.Handler(Context, As(username), photos ?? new FakePhotoAccessor(), Mapper)
                .Handle(new Application.Places.Create.Command { Place = input }, CancellationToken.None);
            Assert.True(result.IsSuccess, result.Error);
            return result.Value.Id;
        }

        protected async Task<Guid> ReviewAs(string username, Guid placeId, int rating = 5, FakePhotoAccessor photos = null, IFormFile file = null)
        {
            var result = await new Application.Visits.Create.Handler(Context, As(username), photos ?? new FakePhotoAccessor(), Mapper)
                .Handle(new Application.Visits.Create.Command
                {
                    Visit = new VisitInput { PlaceId = placeId, Title = "Visit", Description = "It was fine", Rating = rating, File = file }
                }, CancellationToken.None);
            Assert.True(result.IsSuccess, result.Error);
            return result.Value.Id;
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
        public bool Admin { get; set; }
        public string GetUsername() => Username;
        public bool IsAdmin() => Admin;
    }

    public class FakePhotoAccessor : IPhotoAccessor
    {
        public List<string> Deleted { get; } = new();

        private static PhotoUploadResult Result() =>
            new PhotoUploadResult { PublicId = "fake-" + Guid.NewGuid().ToString("N"), Url = "https://example.com/photo.jpg" };
        public Task<PhotoUploadResult> AddPhoto(IFormFile file) => Task.FromResult(Result());
        public Task<PhotoUploadResult> AddPhotoLarge(Stream streamdata, string FileName) => Task.FromResult(Result());
        public Task<PhotoUploadResult> AddPhotoLargeFile(IFormFile file) => Task.FromResult(Result());
        public Task<string> DeletePhoto(string publicId)
        {
            Deleted.Add(publicId);
            return Task.FromResult("ok");
        }

        public static IFormFile Image(string name = "photo.jpg")
        {
            var bytes = new byte[] { 1, 2, 3 };
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "File", name)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }
    }
}
