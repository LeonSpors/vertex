using FluentValidation;
using Vertex.Application.Contracts;

namespace Vertex.Application.Validation;

public sealed class DeployApplicationRequestValidator : AbstractValidator<DeployApplicationRequest>
{
    public DeployApplicationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(63);
        RuleFor(x => x.Namespace).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(63);
        RuleFor(x => x.Image).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Replicas).InclusiveBetween(0, 100);
        RuleFor(x => x.Port).InclusiveBetween(1, 65535);
        RuleFor(x => x.IngressHost).MaximumLength(253).When(x => x.IngressHost is not null);
    }
}

public sealed class ScaleApplicationRequestValidator : AbstractValidator<ScaleApplicationRequest>
{
    public ScaleApplicationRequestValidator() => RuleFor(x => x.Replicas).InclusiveBetween(0, 100);
}

public sealed class CreateEnvironmentRequestValidator : AbstractValidator<CreateEnvironmentRequest>
{
    public CreateEnvironmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(40);
        RuleFor(x => x.Namespace).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(63);
    }
}

public sealed class UpsertSecretRequestValidator : AbstractValidator<UpsertSecretRequest>
{
    public UpsertSecretRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(63);
        RuleFor(x => x.Namespace).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(63);
        RuleFor(x => x.Values).NotEmpty();
    }
}

public sealed class CreateDatabaseRequestValidator : AbstractValidator<CreateDatabaseRequest>
{
    public CreateDatabaseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(40);
        RuleFor(x => x.Namespace).NotEmpty().Matches("^[a-z0-9-]+$").MaximumLength(63);
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

