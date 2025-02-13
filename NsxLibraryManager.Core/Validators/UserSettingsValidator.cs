using FluentValidation;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Core.Validators;

public class UserSettingsValidator : AbstractValidator<UserSettings>
{
    public UserSettingsValidator()
    {
        RuleFor(x => x.ProdKeys)
            .Custom((path, context) =>
            {
                if (path != string.Empty && !File.Exists(path))
                {
                    context.AddFailure("prod.keys file must exist or leave empty to look in home folder.");
                }
            });        

        RuleFor(x => x.LibraryPath)
            .NotEmpty().WithMessage("Library path cannot be empty.");

        RuleFor(x => x.LibraryPath)
            .Custom((path, context) =>
            {
                if (!Directory.Exists(path))
                {
                    context.AddFailure("Library path does not exist.");
                }
            });
        
        RuleFor(x => x.BackupPath)
            .Custom((path, context) =>
            {
                if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                {
                    context.AddFailure("Backup path does not exist, leave empty to avoid using backups.");
                }
            });

        RuleFor(x => x.DownloadSettings)
            .NotNull().WithMessage("Download settings cannot be null.");

    }

}