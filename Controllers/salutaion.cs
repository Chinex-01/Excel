using nameService;
using Microsoft.AspNetCore.Mvc;

namespace Greeting.Controllers
{
    [ApiController]

    [Route("api/Name")]
    public class NameController : Controller
    {
        [HttpPost]
        public string Namee(string name)
        {
            validname validator = new validname();

            if (!validator.IsValid(name))
            {
                return "Invalid name.";
            }
            return $"Welcome {name}";
        }
    }

}