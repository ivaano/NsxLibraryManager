using FluentValidation;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Validators;

public class UserSettingsValidator : AbstractValidator<UserSettings>
{
    public UserSettingsValidator()
    {
        /*
        RuleFor(x => x.TitleDatabase)
            .NotEmpty().WithMessage("Title database path cannot be empty.");

        RuleFor(x => x.TitleDatabase)
            .Custom((path, context) =>
            {
                if (!File.Exists(path))
                {
                    context.AddFailure("Title database does not exist.");
                }
            });
            */
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

        RuleFor(x => x.DownloadSettings)
            .NotNull().WithMessage("Download settings cannot be null.");
/*
        RuleFor(x => x.DownloadSettings.TitleDbPath)
            .NotEmpty().WithMessage("TitleDb path cannot be empty.");

        RuleFor(x => x.DownloadSettings.TitleDbPath)
            .Custom((path, context) =>
            {
                if (!Directory.Exists(path))
                {
                    context.AddFailure("TitleDb path does not exist.");
                }
            });

        RuleFor(x => x.DownloadSettings.TimeoutInSeconds)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0.");

        RuleFor(x => x.DownloadSettings.RegionUrl)
            .NotEmpty().WithMessage("Region url cannot be empty.");

        RuleFor(x => x.DownloadSettings.CnmtsUrl)
            .NotEmpty().WithMessage("Cnmts url cannot be empty.");

        RuleFor(x => x.DownloadSettings.VersionsUrl)
            .NotEmpty().WithMessage("Versions url cannot be empty.");

        RuleFor(x => x.DownloadSettings.Regions)
            .NotEmpty().WithMessage("Regions cannot be empty.");
            */
    }

}