using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) ||
                       p.ParameterType == typeof(IEnumerable<IFormFile>) ||
                       p.ParameterType == typeof(IFormFileCollection))
            .ToList();

        if (!fileParams.Any())
            return;

        // Xóa các parameters được tạo tự động cho form file
        operation.Parameters = operation.Parameters
            .Where(p => !fileParams.Any(fp => fp.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Tạo request body mới
        var properties = new Dictionary<string, OpenApiSchema>();

        foreach (var param in fileParams)
        {
            if (param.ParameterType == typeof(IFormFile))
            {
                properties[param.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else
            {
                properties[param.Name] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
        }

        operation.RequestBody = new OpenApiRequestBody
        {
            Required = true,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = properties
                    }
                }
            }
        };
    }
}