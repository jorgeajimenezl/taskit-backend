using Microsoft.AspNetCore.Mvc;

namespace Taskit.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase { }