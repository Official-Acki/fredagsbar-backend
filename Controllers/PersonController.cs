using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PersonController : Controller
{

    // // 
    // // GET: /HelloWorld/
    // public string Index()
    // {
    //     return "This is my default action...";
    // }
    // 
    // GET: /Person/get/ 
    [HttpGet("get/{id?}")]
    public string get(string? id)
    {
        // It has an id parameter
        if (string.IsNullOrEmpty(id))
        {
            return "No id provided";
        }
        else
        {
            bool parsed = int.TryParse(id, out int parsedId);
            if (!parsed || parsedId <= 0)
            {
                return "Invalid id provided";
            }
            // Return json
            var person = DatabaseController.Instance.GetPerson(int.Parse(id));
            if (person == null)
            {
                return "No person found with id " + id;
            }
            else
            {
                return System.Text.Json.JsonSerializer.Serialize(person);
            }
        }
    }
}