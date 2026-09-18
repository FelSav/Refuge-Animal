using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiMuniChien_V1_Proprietaire.models;

[Route("api/Trains")]
[ApiController]
public class TrainsController : ControllerBase
{
    private readonly TrainContext _context;
    public TrainsController(TrainContext context)
    {
        _context = context;
    }


    // GET: api/Trains
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Train>>> GetTrains()
    {
        //return await _context.Trains.ToListAsync();

        Console.WriteLine(">>> Requête GET /api/Train reçue");

        var trains = await _context.Trains.ToListAsync();

        Console.WriteLine($">>> Nombre de trains récupérés : {trains.Count}");

        foreach (var train in trains)
        {
            Console.WriteLine(
                $">>> Train : Id={train.id}, Model={train.model}, Couleur={train.couleur}, EnStock={train.enstock}"
            );
        }

        return trains;
    }

    // GET: api/Trains/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Train>> GetTrains(int id)
    {
        var Trains = await _context.Trains.FindAsync(id);

        if (Trains == null)
        {

            return NotFound();
        }

        return Trains;
    }

    // PUT: api/Trains/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTrains(int? id, Train Trains)
    {
        if (id != Trains.id)
        {
            return BadRequest();
        }

        _context.Entry(Trains).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TrainsExists(id))
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

    // POST: api/Trains
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Train>> PostTrains(Train Trains)
    {
        _context.Trains.Add(Trains);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetTrains", new { id = Trains.id }, Trains);
    }

    // DELETE: api/Trains/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTrains(int? id)
    {
        var Trains = await _context.Trains.FindAsync(id);
        if (Trains == null)
        {
            return NotFound();
        }

        _context.Trains.Remove(Trains);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TrainsExists(int? id)
    {
        return _context.Trains.Any(e => e.id == id);
    }
}
