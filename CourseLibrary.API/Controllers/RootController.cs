using CourseLibrary.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController : ControllerBase
    {
        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            // create HATEOAS for root
            var links = new List<LinkDto>();

            links.Add(new LinkDto
            {
                Href = Url.Link("GetRoot", null),
                Rel = "self",
                Method = "GET"
            });

            links.Add(new LinkDto
            {
                Href = Url.Link("GetAuthors", null),
                Rel = "get_authors",
                Method = "GET"
            });

            links.Add(new LinkDto
            {
                Href = Url.Link("CreateAuthor", null),
                Rel = "create_author",
                Method = "POST"
            });

            return Ok(links);
        }
    }
}
