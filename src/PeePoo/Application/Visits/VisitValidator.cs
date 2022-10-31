using Application.Visits;
using FluentValidation;

namespace Application.PlaceVisits
{
    public class VisitValidator : AbstractValidator<VisitDto>
    {
        public VisitValidator()
        {
            RuleFor(x => x.Id).NotEmpty().NotNull();
            RuleFor(x => x.PlaceId).NotEmpty().NotNull();
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
        }
    }

}