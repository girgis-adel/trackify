using Application.Features.Artists;
using Application.Features.Demo;
using Application.Features.Distributions;
using Application.Features.DSPs;
using Application.Features.Identity;
using Application.Features.Tracks;
using Application.Features.UI;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Application;

public static class DependencyInjection
{
    public static IHostApplicationBuilder ConfigureApplicationLayer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        builder.Services.AddScoped<IArtistService, ArtistService>();
        builder.Services.AddScoped<ITrackService, TrackService>();
        builder.Services.AddScoped<IDistributionService, DistributionService>();
        builder.Services.AddScoped<IDSPService, DSPService>();
        builder.Services.AddScoped<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IDemoService, DemoService>();
        builder.Services.AddScoped<IUIService, UIService>();

        return builder;
    }
}
