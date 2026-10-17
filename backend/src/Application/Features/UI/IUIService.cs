using Application.Features.UI.Dtos;

namespace Application.Features.UI;

public interface IUIService
{
    Task<BootstrapResponse> GetBootstrapAsync(CancellationToken cancellationToken = default);
}
