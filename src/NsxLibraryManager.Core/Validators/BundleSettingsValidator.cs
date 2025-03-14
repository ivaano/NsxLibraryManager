using FluentValidation;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Core.Validators;

public class BundleSettingsValidator : AbstractValidator<BundleRenamerSettings>
{
    public BundleSettingsValidator()
    {
        RuleFor(x => x.InputPath)
            .NotEmpty().WithMessage("Input path cannot be empty.");

        RuleFor(x => x.InputPath)
            .Custom((path, context) =>
            {
                if (!Directory.Exists(path))
                {
                    context.AddFailure("Directory does not exist.");
                }
            });
        RuleFor(x => x.OutputBasePath)
            .Custom((path, context) =>
            {
                if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                {
                    context.AddFailure("Directory does not exist.");
                }
            });
    }
}