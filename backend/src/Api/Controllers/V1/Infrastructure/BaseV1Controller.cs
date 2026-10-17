using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.V1.Infrastructure;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseV1Controller : ControllerBase;
