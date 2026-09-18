using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMuniChien_V1_Proprietaire.models;

[Route("api/")]
[ApiController]
public class GlobalController : ControllerBase
{
    // GET: api/ 
    /// qui renvoie ok a la requete
    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
        return Ok("ok");
    }
}