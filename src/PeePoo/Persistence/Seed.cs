using Domain;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence
{
    public static class Seed
    {
        public const string AdminRole = "Admin";
        public const string DemoPassword = "Pa$$w0rd";

        /// <summary>Runs in every environment: the Admin role, plus an admin account when one is configured.</summary>
        public static async Task EnsureRolesAndAdminAsync(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, string adminEmail, string adminPassword)
        {
            if (!await roleManager.RoleExistsAsync(AdminRole))
                await roleManager.CreateAsync(new IdentityRole(AdminRole));

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                return;

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser { DisplayName = "Moderación", UserName = "admin", Email = adminEmail };
                var created = await userManager.CreateAsync(admin, adminPassword);
                if (!created.Succeeded) return;
            }
            if (!await userManager.IsInRoleAsync(admin, AdminRole))
                await userManager.AddToRoleAsync(admin, AdminRole);
        }

        /// <summary>Demo data for local development: fictional spots around Santo Domingo.</summary>
        public static async Task SeedDemoDataAsync(DataContext context, UserManager<ApplicationUser> userManager)
        {
            if (context.Places.Any()) return;

            var people = new[]
            {
                ("starling", "Starling", "Recorro la Zona Colonial a pie casi todos los días."),
                ("alfredo", "Alfredo", "Papá de dos. Siempre buscando cambiadores."),
                ("germosen", "Germosen", "Uso silla de ruedas; reviso accesos y barras de apoyo."),
                ("mariela", "Mariela", "Corredora del Malecón."),
                ("kevin", "Kevin", "Repartidor en moto, conozco media ciudad."),
            };

            var users = new List<ApplicationUser>();
            foreach (var (username, name, bio) in people)
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    DisplayName = name,
                    Bio = bio,
                    Email = $"{username}@test.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-120)
                };
                await userManager.CreateAsync(user, DemoPassword);
                users.Add(user);
            }

            var now = DateTime.UtcNow;
            var places = new List<Place>
            {
                P("Café Las Damas (demo)", "Unisex", "Calle Las Damas, Zona Colonial", 18.4739, -69.8836,
                  "Baño al fondo del patio interior. Hay que pedir la llave en la barra.", "Clientes. Pedir llave en la barra.",
                  "8:00–20:00", toilets: 1, urinals: 0, free: false, roomy: false, baby: false, accessible: false,
                  rating: 4, owner: users[0], created: now.AddDays(-90), verified: now.AddDays(-3)),
                P("Plaza Comercial El Conde – nivel 1 (demo)", "Unisex", "Calle El Conde, Zona Colonial", 18.4725, -69.8879,
                  "Baños del centro comercial, junto a las escaleras eléctricas. Limpieza frecuente.", null,
                  "9:00–21:00", toilets: 6, urinals: 4, free: true, roomy: true, baby: true, accessible: true,
                  rating: 5, owner: users[0], created: now.AddDays(-80), verified: now.AddDays(-1)),
                P("Parque del Malecón – módulo de baños (demo)", "Unisex", "Av. George Washington, Malecón", 18.4635, -69.9010,
                  "Módulo público frente al mar. Lleva papel por si acaso.", "A veces cierra temprano los lunes.",
                  "7:00–19:00", toilets: 4, urinals: 2, free: true, roomy: false, baby: false, accessible: true,
                  rating: 3, owner: users[3], created: now.AddDays(-75), verified: now.AddDays(-10)),
                P("Estación de combustible Av. 27 (demo)", "Unisex", "Av. 27 de Febrero esq. Av. Abraham Lincoln", 18.4747, -69.9318,
                  "Baño de la tienda de conveniencia, abierto 24 horas.", "Pedir la llave al cajero.",
                  "24 horas", toilets: 2, urinals: 1, free: true, roomy: false, baby: false, accessible: false,
                  rating: 3, owner: users[4], created: now.AddDays(-60), verified: now.AddDays(-6)),
                P("Centro Comercial Piantini – planta alta (demo)", "Family", "Piantini", 18.4703, -69.9395,
                  "Sala familiar con cambiador, sillita para niños y lavamanos bajo.", null,
                  "10:00–21:00", toilets: 8, urinals: 4, free: true, roomy: true, baby: true, accessible: true,
                  rating: 5, owner: users[1], created: now.AddDays(-55), verified: now.AddDays(-2)),
                P("Biblioteca pública de Gazcue (demo)", "Unisex", "Gazcue", 18.4675, -69.9070,
                  "Baños en el primer piso. Silenciosos y limpios.", "Registrarse en la entrada.",
                  "9:00–17:00 (lun–vie)", toilets: 3, urinals: 1, free: true, roomy: false, baby: false, accessible: true,
                  rating: 4, owner: users[2], created: now.AddDays(-50), verified: now.AddDays(-20)),
                P("Mercado de Naco – baños (demo)", "Unisex", "Naco", 18.4876, -69.9302,
                  "Baños del mercado, en la parte trasera.", "Cobra una tarifa pequeña.",
                  "7:00–18:00", toilets: 2, urinals: 2, free: false, roomy: false, baby: false, accessible: false,
                  rating: 2, owner: users[4], created: now.AddDays(-40), verified: now.AddDays(-30)),
                P("Parque Mirador Sur – entrada Av. Anacaona (demo)", "Unisex", "Mirador Sur", 18.4447, -69.9550,
                  "Baños cerca del área de juegos infantiles.", "Mejor temprano en la mañana.",
                  "6:00–18:00", toilets: 4, urinals: 2, free: true, roomy: true, baby: true, accessible: true,
                  rating: 4, owner: users[3], created: now.AddDays(-35), verified: now.AddDays(-4)),
                P("Hospital docente – sala de espera (demo)", "Accessible", "Av. Pedro Henríquez Ureña", 18.4790, -69.9180,
                  "Baño accesible con barras de apoyo y puerta ancha.", "Uso para pacientes y acompañantes.",
                  "24 horas", toilets: 1, urinals: 0, free: true, roomy: true, baby: false, accessible: true,
                  rating: 4, owner: users[2], created: now.AddDays(-30), verified: now.AddDays(-8)),
                P("Supermercado Bella Vista (demo)", "Women", "Bella Vista", 18.4560, -69.9440,
                  "Baño de damas junto a atención al cliente.", null,
                  "8:00–22:00", toilets: 3, urinals: 0, free: true, roomy: false, baby: true, accessible: false,
                  rating: 4, owner: users[1], created: now.AddDays(-20), verified: now.AddDays(-5)),
                P("Terminal de autobuses del norte (demo)", "Men", "Av. Máximo Gómez", 18.4950, -69.9100,
                  "Baño de caballeros en el andén 2.", "Concurrido en horas pico.",
                  "5:00–22:00", toilets: 3, urinals: 5, free: false, roomy: false, baby: false, accessible: false,
                  rating: 2, owner: users[4], created: now.AddDays(-12), verified: null),
                P("Café de la Universidad (demo)", "Unisex", "Ciudad Universitaria", 18.4610, -69.9200,
                  "Baño pequeño pero impecable. Buen wifi mientras esperas.", null,
                  "7:30–19:00", toilets: 1, urinals: 0, free: false, roomy: false, baby: false, accessible: false,
                  rating: 5, owner: users[0], created: now.AddDays(-5), verified: now.AddDays(-1)),
            };
            context.Places.AddRange(places);

            var reviews = new (int place, int user, int rating, string title, string text, int daysAgo)[]
            {
                (0, 3, 4, "Limpio y tranquilo", "Hay que pedir la llave pero vale la pena. Tenían jabón y papel.", 20),
                (0, 4, 3, "Algo pequeño", "Funciona bien, solo que hay que consumir algo primero.", 9),
                (1, 1, 5, "El mejor de la zona", "Cambiador limpio y espacio para el coche del bebé.", 15),
                (1, 2, 5, "Accesible de verdad", "Rampa, puerta ancha y barras a los dos lados.", 7),
                (1, 3, 4, "Muy bien", "Un poco lleno los sábados, pero siempre limpio.", 2),
                (2, 0, 3, "Cumple", "Abierto cuando lo necesité. Faltaba papel.", 11),
                (2, 4, 2, "Cerrado los lunes", "Fui un lunes a las 4 pm y ya estaba cerrado.", 4),
                (3, 0, 3, "Salvavidas a las 2 am", "No es lujo, pero está abierto toda la noche.", 30),
                (4, 1, 5, "Pensado para familias", "La sala familiar es amplia y tiene sillita para el niño.", 6),
                (5, 2, 4, "Silencioso", "Accesible por el ascensor del lado izquierdo.", 18),
                (6, 0, 2, "Mejorable", "Cobran y no siempre está limpio. Solo en emergencia.", 25),
                (7, 3, 4, "Bien para corredores", "Después de correr en la mañana estaba impecable.", 3),
                (7, 1, 4, "Con cambiador", "El cambiador está en el baño de damas y en el familiar.", 8),
                (8, 2, 4, "Barras firmes", "Buen espacio para girar con la silla.", 14),
                (11, 3, 5, "Impecable", "Pequeño pero lo limpian cada hora.", 1),
            };
            foreach (var r in reviews)
            {
                context.Visits.Add(new Visit
                {
                    Id = Guid.NewGuid(),
                    Place = places[r.place],
                    Author = users[r.user],
                    Rating = r.rating,
                    Title = r.title,
                    Description = r.text,
                    CreatedAt = now.AddDays(-r.daysAgo)
                });
            }

            // A few saved places so "Saved" isn't empty for the test user.
            context.FavoritePlaces.Add(new FavoritePlace { Place = places[4], User = users[0] });
            context.FavoritePlaces.Add(new FavoritePlace { Place = places[7], User = users[0] });

            await context.SaveChangesAsync();
        }

        private static Place P(string name, string type, string address, double lat, double lng,
            string description, string observations, string hours, int toilets, int urinals, bool free,
            bool roomy, bool baby, bool accessible, int rating, ApplicationUser owner, DateTime created, DateTime? verified)
        {
            var place = new Place
            {
                Id = Guid.NewGuid(),
                Name = name,
                Type = type,
                Address = address,
                Lat = lat,
                Long = lng,
                Description = description,
                Observations = observations,
                OpeningHours = hours,
                Toilets = toilets,
                Urinals = urinals,
                IsFree = free,
                IsRoomy = roomy,
                HaveBabyChanger = baby,
                IsAccessible = accessible,
                IsAvailable = true,
                IsAproved = true,
                Rating = rating,
                CreatedAt = created,
                LastVerifiedAt = verified
            };
            place.Favorites.Add(new FavoritePlace { Place = place, User = owner, IsOwner = true });
            return place;
        }
    }
}
