using Microsoft.AspNetCore.Mvc;

namespace Taskit.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase { }