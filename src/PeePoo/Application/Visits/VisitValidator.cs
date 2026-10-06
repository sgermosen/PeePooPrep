using Application.Core;
using FluentValidation;

namespace Application.Visits
{
    public class VisitValidator : AbstractValidator<VisitInput>
    {
        public VisitValidator()
        {
            RuleFor(x => x.PlaceId).NotEmpty();
            RuleFor(x => x.Title).NotEmpty().WithMessage("Add a short title.").MaximumLength(80);
            RuleFor(x => x.Description).NotEmpty().WithMessage("Tell people how it was.").MaximumLength(1000);
            RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
            RuleFor(x => x.File).Must(PhotoRules.IsAcceptable).WithMessage(PhotoRules.Message);
        }
    }

    public class VisitEditValidator : AbstractValidator<VisitEditInput>
    {
        public VisitEditValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(80);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        }
    }
}
