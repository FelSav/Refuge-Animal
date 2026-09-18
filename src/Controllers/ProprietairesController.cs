using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMuniChien_V1_Proprietaire.models;

[Route("api/[controller]")]
[ApiController]
public class ProprietairesController : ControllerBase
{
    private readonly ProprietairesContext _context;
    public ProprietairesController(ProprietairesContext context)
    {
        _context = context;
    }


    // GET: api/Proprietaires
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Proprietaires>>> GetProprietaires()
    {
        
        return await _context.Proprietaires.ToListAsync();
    }

    // GET: api/Proprietaires/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Proprietaires>> GetProprietaires(int id)
    {
        var proprietaires = await _context.Proprietaires.FindAsync(id);

        if (proprietaires == null)
        {

            return NotFound();
        }

        return proprietaires;
    }

    // PUT: api/Proprietaires/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProprietaires(int? id, Proprietaires proprietaires)
    {
        if (id != proprietaires.Id)
        {
            return BadRequest();
        }

        _context.Entry(proprietaires).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProprietairesExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Proprietaires
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Proprietaires>> PostProprietaires(Proprietaires proprietaires)
    {
        _context.Proprietaires.Add(proprietaires);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProprietaires", new { id = proprietaires.Id }, proprietaires);
    }

    // DELETE: api/Proprietaires/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProprietaires(int? id)
    {
        var proprietaires = await _context.Proprietaires.FindAsync(id);
        if (proprietaires == null)
        {
            return NotFound();
        }

        _context.Proprietaires.Remove(proprietaires);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProprietairesExists(int? id)
    {
        return _context.Proprietaires.Any(e => e.Id == id);
    }
}
