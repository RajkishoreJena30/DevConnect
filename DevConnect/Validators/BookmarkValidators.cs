using DevConnect.DTOs;
using FluentValidation;

namespace DevConnect.Validators
{
    public class BookmarkQueryValidator : AbstractValidator<BookmarkQueryParams>
    {
        public BookmarkQueryValidator()
        {
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.SortBy)
                .Must(s => s is "createdAt" or "title")
                .WithMessage("SortBy must be 'createdAt' or 'title'.");
            RuleFor(x => x.Search)
                .MaximumLength(100).When(x => x.Search != null);
        }
    }
}