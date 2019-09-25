using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly DataContext _context;
        public ValuesController(DataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var values = await _context.Values.ToListAsync();
            return Ok(values);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var value = await _context.Values.FirstOrDefaultAsync(v => v.Id == id);
            return Ok(value);

        }
    }
}
