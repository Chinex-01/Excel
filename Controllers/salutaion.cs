using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nameService;

namespace Greeting.Controllers
{
    [ApiController]

    [Route("api/Name")]
    [Authorize]
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