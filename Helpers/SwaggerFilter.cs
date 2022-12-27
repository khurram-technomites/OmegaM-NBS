using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace NowBuySell.Web.Helpers
{
#pragma warning disable 1591
    public class SwaggerFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            var pathsToRemove = swaggerDoc.paths
                .Where(pathItem => !pathItem.Key.Contains("api/"))
                .ToList();

            foreach (var item in pathsToRemove)
            {
                swaggerDoc.paths.Remove(item.Key);
            }
        }
    }
}