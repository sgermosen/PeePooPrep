using Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence
{
    public class Seed
    {
        public static async Task SeedData(DataContext context,
            UserManager<ApplicationUser> userManager)
        {
            if (!userManager.Users.Any() && !context.Places.Any())
            {
                var users = new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        DisplayName = "Starling",
                        UserName = "starling",
                        Email = "starling@test.com"
                    },
                    new ApplicationUser
                    {
                        DisplayName = "Alfredo",
                        UserName = "alfredo",
                        Email = "alfredo@test.com"
                    },
                    new ApplicationUser
                    {
                        DisplayName = "Germosen",
                        UserName = "germosen",
                        Email = "germosen@test.com"
                    },
                };

                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                }

                var activities = new List<Place>
                {
                    new Place
                    {
                        Name = "Past Place 1",
                        CreatedAt = DateTime.Now.AddMonths(-2),
                        Description = "Place 2 months ago",
                        Type = "Unisex",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[0],
                                IsOwner = true
                            }
                        }
                    },
                    new Place
                    {
                        Name = "Past Place 2",
                        CreatedAt = DateTime.Now.AddMonths(-1),
                        Description = "Place 1 month ago",
                        Type = "Mens Only",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[0],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 1",
                        CreatedAt = DateTime.Now.AddMonths(1),
                        Description = "Place 1 month in future",
                        Type = "Womens Only",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[2],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 2",
                        CreatedAt = DateTime.Now.AddMonths(2),
                        Description = "Place 2 months in future",
                        Type = "Mens",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[0],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[2],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 3",
                        CreatedAt = DateTime.Now.AddMonths(3),
                        Description = "Place 3 months in future",
                        Type = "Unisex",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[0],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 4",
                        CreatedAt = DateTime.Now.AddMonths(4),
                        Description = "Place 4 months in future",
                        Type = "Mens Only",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = true
                            }
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 5",
                        CreatedAt = DateTime.Now.AddMonths(5),
                        Description = "Place 5 months in future",
                        Type = "Unisex",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[0],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 6",
                        CreatedAt = DateTime.Now.AddMonths(6),
                        Description = "Place 6 months in future",
                        Type = "Womens Only",

                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[2],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 7",
                        CreatedAt = DateTime.Now.AddMonths(7),
                        Description = "Place 7 months in future",
                        Type = "Womens",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[0],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[2],
                                IsOwner = false
                            },
                        }
                    },
                    new Place
                    {
                        Name = "Future Place 8",
                        CreatedAt = DateTime.Now.AddMonths(8),
                        Description = "Place 8 months in future",
                        Type = "Unisex",
                        Favorites = new List<FavoritePlace>
                        {
                            new FavoritePlace
                            {
                                User = users[2],
                                IsOwner = true
                            },
                            new FavoritePlace
                            {
                                User = users[1],
                                IsOwner = false
                            },
                        }
                    }
                };

                await context.Places.AddRangeAsync(activities);
                await context.SaveChangesAsync();
            }
        }
    }
}
