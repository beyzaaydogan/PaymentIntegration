using System.Text.Json;
using FluentValidation;

namespace PaymentIntegration.API.Middlewares;

public class ValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        if (context.Request.Method == HttpMethods.Post || context.Request.Method == HttpMethods.Put)
        {
            context.Request.EnableBuffering();

            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                var endpoint = context.GetEndpoint();
                var parameter = endpoint?.Metadata
                    .OfType<Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor>()
                    .FirstOrDefault()?
                    .Parameters
                    .FirstOrDefault();

                if (parameter != null)
                {
                    var modelType = parameter.ParameterType;
                    var model = JsonSerializer.Deserialize(body, modelType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (model != null)
                    {
                        var validatorType = typeof(IValidator<>).MakeGenericType(modelType);
                        var validator = serviceProvider.GetService(validatorType) as IValidator;

                        if (validator != null)
                        {
                            var contextValidation = new ValidationContext<object>(model);
                            var result = await validator.ValidateAsync(contextValidation);

                            if (!result.IsValid)
                            {
                                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                context.Response.ContentType = "application/json";
                                var errorResponse = JsonSerializer.Serialize(new
                                {
                                    Message = "Validation failed",
                                    Errors = result.Errors.Select(x => x.ErrorMessage)
                                });
                                await context.Response.WriteAsync(errorResponse);
                                return;
                            }
                        }
                    }
                }
            }
        }

        await _next(context);
    }
}
