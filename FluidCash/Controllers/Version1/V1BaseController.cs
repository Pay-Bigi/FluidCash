using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FluidCash.Controllers.Version1;

[ApiVersion(1.0)]
[ApiController]
[Route("[controller]")]
public class V1BaseController : ControllerBase
{
}
