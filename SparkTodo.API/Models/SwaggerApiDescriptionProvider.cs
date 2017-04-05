using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SparkTodo.API.Models
{
    /// <summary>
    /// 为修复Swagger错误自定义ApiDescriptionProvider
    /// </summary>
    public class SwaggerApiDescriptionProvider : IApiDescriptionProvider
    {
        private readonly ModelMetadata @string;
        /// <summary>
        /// SwaggerApiDescriptionProvider ctor
        /// </summary>
        /// <param name="metadadataProvider"></param>
        public SwaggerApiDescriptionProvider(IModelMetadataProvider metadadataProvider) =>
            @string = metadadataProvider.GetMetadataForType(typeof(string));

        /// <summary>
        /// Order
        /// </summary>
        public int Order => 0;

        /// <summary>
        /// OnProvidersExecuted
        /// </summary>
        /// <param name="context">ApiDescriptionProviderContext</param>
        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
            foreach (var result in context.Results)
            {
                foreach (var parameter in result.ParameterDescriptions)
                {
                    if (parameter.ModelMetadata == null)
                    {
                        parameter.ModelMetadata = @string;
                    }
                }
            }
        }

        /// <summary>
        /// OnProvidersExecuting
        /// </summary>
        /// <param name="context">ApiDescriptionProviderContext</param>
        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
        }
    }
}