using Microsoft.AspNetCore.Mvc;

namespace ASM_SIMS.Controllers
{
    public class DemoController : Controller
    {
        public IActionResult Index()
        {
            //return View();
            return Ok("Hello World, this is DemoController Page");
            //Domain/Demo/Index

        }

        public IActionResult Test(int ID, string name) // truyen tham so bat buoc
        {
            return Ok("Hello World, ID: " + ID + "\n Name: " + name);
        }
    }
}
