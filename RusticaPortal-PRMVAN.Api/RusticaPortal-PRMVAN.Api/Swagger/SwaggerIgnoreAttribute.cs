using System;

namespace ArellanoCore.Api.Swagger
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }
}
