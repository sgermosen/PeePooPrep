using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Site
{
    public record Guide(string Slug, string Title, string Summary, DateTime Published, int Minutes);

    /// <summary>Editorial guides. Each body lives in Pages/Guias/Articulos/_{Slug}.cshtml.</summary>
    public static class Guides
    {
        public static readonly IReadOnlyList<Guide> All = new List<Guide>
        {
            new("como-encontrar-un-bano-limpio",
                "Cómo encontrar un baño limpio cuando estás fuera de casa",
                "Dónde suele haber baños disponibles, qué señales mirar antes de entrar y cómo pedir permiso sin pasar vergüenza.",
                new DateTime(2026, 9, 2), 5),
            new("banos-accesibles-que-revisar",
                "Baños accesibles: qué revisar de verdad",
                "Un cartel con una silla de ruedas no garantiza nada. Medidas, barras, puertas y detalles que hacen la diferencia.",
                new DateTime(2026, 9, 9), 6),
            new("salir-con-bebes",
                "Salir con bebés: cambiadores, salas familiares y plan B",
                "Qué llevar en la pañalera, cómo identificar un buen cambiador y qué hacer cuando no hay ninguno cerca.",
                new DateTime(2026, 9, 16), 5),
            new("como-escribir-una-resena-util",
                "Cómo escribir una reseña que de verdad ayude",
                "Los cinco datos que más buscan las personas antes de ir, y por qué «estaba bien» no ayuda a nadie.",
                new DateTime(2026, 9, 23), 4),
        };

        public static Guide Find(string slug) => All.FirstOrDefault(g => g.Slug == slug);
    }
}
