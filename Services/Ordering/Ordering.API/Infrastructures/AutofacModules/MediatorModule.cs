using Autofac;
using FluentValidation;
using MediatR;
using Ordering.API.Applications.Behaviors;
using Ordering.API.Applications.Commands;
using Ordering.API.Applications.Validations;
using System.Reflection;

namespace Ordering.API.Infrastructures.AutofacModules
{
    public class MediatorModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
             .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(CreateOrderCommand).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>));

            builder
             .RegisterAssemblyTypes(typeof(CreateOrderCommandValidator).GetTypeInfo().Assembly)
             .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
             .AsImplementedInterfaces();

            builder.RegisterGeneric(typeof(LoggingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
            builder.RegisterGeneric(typeof(ValidatorBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}
