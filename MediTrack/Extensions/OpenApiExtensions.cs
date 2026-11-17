using Microsoft.OpenApi;

namespace MediTrack.Extensions
{
    public static class OpenApiExtensions
    {
        //open api dokümanı üretir
        //ve dokümana jwt güvenlik şemasını ekler
        //Bu sayede swagger da sağ üstte authorize butonu çıkar
        //oraya girelen token da her isteğe "Authorization: Bearer <token>" header'ı olarak eklenir
        public static IServiceCollection AddOpenApiWithJwt(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                //OpenAPI dokümanı üretildikten sonra, tarayıcıya gönderilmeden
                //hemen önce dokümanın üzerinde değişiklik yapmanızı sağlıyor.
                //"Alt tire c#'ta _(discard) bu parametre geliyor ama kullanmıyorum demenin yolu
                options.AddDocumentTransformer((document, _, _) =>
                {
                    //Güvenlik şemasını tanımla: token, HTTP Authorization header'ında "Bearer" ile gönderilir.
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                    {
                        ["Bearer"] = new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            Description = "Login'den dönen token'ı yapıştırın (başına 'Bearer' yazmayın)."
                        }

                    };

                    //Şemayı tüm endpointlere uygular(her endpointte kilit simgesi görünür)
                    //[AllowAnonymous] endpoint'ler token olmadan da çalışmaya devam eder.

                    document.Security = [
                              new OpenApiSecurityRequirement{
                                   [new OpenApiSecuritySchemeReference("Bearer", document)] = []


                              }


                        ];
                    return Task.CompletedTask;


                }
                );


            });

            return services;

        }
    }
}