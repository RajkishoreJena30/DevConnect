using DevConnect.DTOs;
using FluentValidation;

namespace DevConnect.Validators
{
    // Validates CreatePostDTO — runs before PostsController.Create() and Update()
    public class CreatePostValidator : AbstractValidator<CreatePostDTO>
    {
        public CreatePostValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MinimumLength(3).WithMessage("Title must be at least 3 characters.")
                .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MinimumLength(10).WithMessage("Content must be at least 10 characters.")
                .MaximumLength(5000).WithMessage("Content cannot exceed 5000 characters.");
        }
    }

    // Validates CreateCommentDTO — runs before CommentsController.AddComment() and UpdateComment()
    public class CreateCommentValidator : AbstractValidator<CreateCommentDTO>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment cannot be empty.")
                .MinimumLength(2).WithMessage("Comment must be at least 2 characters.")
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
        }
    }
}