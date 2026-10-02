using Application.Core;
using FluentValidation;

namespace Application.Places
{
    public class PlaceValidator : AbstractValidator<PlaceInput>
    {
        public PlaceValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Give the place a name.")
                .MaximumLength(80);
            RuleFor(x => x.Type).Must(t => PlaceTypes.Normalize(t) != null)
                .WithMessage("Type must be one of: " + string.Join(", ", PlaceTypes.All) + ".");
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.Observations).MaximumLength(500);
            RuleFor(x => x.Address).MaximumLength(200);
            RuleFor(x => x.OpeningHours).MaximumLength(120);
            RuleFor(x => x.Urinals).InclusiveBetween(0, 50);
            RuleFor(x => x.Toilets).InclusiveBetween(0, 50);
            RuleFor(x => x.Rating).InclusiveBetween(0, 5);
            RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
            RuleFor(x => x.Long).InclusiveBetween(-180, 180);
            RuleFor(x => x).Must(x => x.Lat != 0 || x.Long != 0)
                .WithName("Location").WithMessage("We couldn't get a location for this place. Turn on location and try again.");
            RuleFor(x => x.File).Must(PhotoRules.IsAcceptable).WithMessage(PhotoRules.Message);
        }
    }
}
