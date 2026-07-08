using Microsoft.AspNetCore.Mvc;

namespace Greeting.Controllers
{
    [ApiController]

    [Route("api/Name")]
    public class Name : Controller
    {
        [HttpPost]
        public string Namee(string Name)
        {
            return $"Welcome {Name}";
        }
    }
}

