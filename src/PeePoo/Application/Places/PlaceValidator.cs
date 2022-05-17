using Domain;
using FluentValidation;

namespace Application.Places
{
    public class PlaceValidator : AbstractValidator<Place>
    {
        public PlaceValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.CreatedAt).NotEmpty();
        }
    }

}