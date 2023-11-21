
using AutoMapper;
using CourseLibrary.API.ActionConstraints;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Services;
using CourseLibrary.API.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using System.Dynamic;
using System.Text.Json;

namespace CourseLibrary.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController : ControllerBase
{
    private readonly ICourseLibraryRepository _courseLibraryRepository;
    private readonly IMapper _mapper;
    private readonly IPropertyMappingService _propertyMappingService;
    private readonly IPropertyCheckerService _propertyCheckerService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public AuthorController(
        ICourseLibraryRepository courseLibraryRepository,
        IMapper mapper, IPropertyMappingService propertyMappingService, IPropertyCheckerService propertyCheckerService, ProblemDetailsFactory problemDetailsFactory)
    {
        _courseLibraryRepository = courseLibraryRepository ??
            throw new ArgumentNullException(nameof(courseLibraryRepository));
        _mapper = mapper ??
            throw new ArgumentNullException(nameof(mapper));
        _propertyMappingService = propertyMappingService;
        _propertyCheckerService = propertyCheckerService;
        _problemDetailsFactory = problemDetailsFactory;
    }

    [HttpGet(Name = "GetAuthors")]
    [HttpHead]
    public async Task<IActionResult> GetAuthors([FromQuery] AuthorResourceParameter? authorResourceParameter)
    {

        if (authorResourceParameter is not null && !_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorResourceParameter.OrderBy))
        {
            return BadRequest();
        }

        if (authorResourceParameter is not null && !_propertyCheckerService.TypeHasProperties<AuthorDto>
              (authorResourceParameter.Fields))
        {
            return BadRequest(
                _problemDetailsFactory.CreateProblemDetails(HttpContext,
                    statusCode: 400,
                    detail: $"Not all requested data shaping fields exist on " +
                    $"the resource: {authorResourceParameter.Fields}"));
        }

        // get authors from repo
        var authorsFromRepo = await _courseLibraryRepository
            .GetAuthorsAsync(authorResourceParameter);
      
        var paginationMetadata = new PaginationMetadata
        {
            TotalCount = authorsFromRepo.TotalCount,
            PageSize = authorsFromRepo.PageSize,
            PageNumber = authorsFromRepo.CurrentPage,
            TotalPages = authorsFromRepo.TotalPages         
        };


        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize<PaginationMetadata>(paginationMetadata));

        // create HATEOAS links
        var links = CreateLinksForAuthors(authorResourceParameter, authorsFromRepo.HasNext, authorsFromRepo.HasPrevious);

        var shapedAuthors = _mapper.Map<AuthorDto[]>(authorsFromRepo).ShapeData<AuthorDto>(authorResourceParameter?.Fields);

        var shapedAuthorsWithLinks = shapedAuthors.Select<object, object?>(author =>
        {
            var authorAsDictionary = author as IDictionary<string, object?>;
            if (authorAsDictionary is not null && authorAsDictionary.TryGetValue("Id", out object? authorId) && authorId is not null)
            {
                var authorLinks = CreateLinksForAuthor((Guid)authorId, null);
                authorAsDictionary.Add("links", authorLinks);
            }
            return authorAsDictionary;
        });

        var body = new
        {
            value = shapedAuthorsWithLinks,
            links
        };

        // return them
        return Ok(body);
    }

    private string? CreateAuthorResourceUri(AuthorResourceParameter? authorResourceParameter, ResoureUriType type)
    {
        authorResourceParameter ??= new AuthorResourceParameter();
        switch (type)
        {
            case ResoureUriType.PreviousPage:
                return Url.Link("GetAuthors", new AuthorResourceParameter
                {
                    Fields = authorResourceParameter.Fields,
                    OrderBy = authorResourceParameter.OrderBy,
                    PageSize = authorResourceParameter.PageSize,
                    PageNumber = authorResourceParameter.PageNumber - 1,
                    MainCategory = authorResourceParameter.MainCategory,
                    SearchQuery = authorResourceParameter.SearchQuery
                });
            case ResoureUriType.NextPage:
                return Url.Link("GetAuthors", new AuthorResourceParameter
                {
                    Fields = authorResourceParameter.Fields,
                    OrderBy = authorResourceParameter.OrderBy,
                    PageSize = authorResourceParameter.PageSize,
                    PageNumber = authorResourceParameter.PageNumber + 1,
                    MainCategory = authorResourceParameter.MainCategory,
                    SearchQuery = authorResourceParameter.SearchQuery
                });
            case ResoureUriType.CurrentPage:
            default:
                return Url.Link("GetAuthors", new AuthorResourceParameter
                {
                    Fields = authorResourceParameter.Fields,
                    OrderBy = authorResourceParameter.OrderBy,
                    PageSize = authorResourceParameter.PageSize,
                    PageNumber = authorResourceParameter.PageNumber - 1,
                    MainCategory = authorResourceParameter.MainCategory,
                    SearchQuery = authorResourceParameter.SearchQuery
                });
        }
    }

    [RequestHeaderMatchesMediaType("Accept", "application/json", "application/vnd.marvin.author.friendly+json")]
    [Produces("application/json", "application/vnd.marvin.author.friendly+json")]
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetAuthorWithoutLinks(Guid authorId, string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 400, detail: $"Not all requested data shaping fields exist on the resource: {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo is null)
        {
            return NotFound();
        }

        // friendly author
        var friendlyResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields) as IDictionary<string, object>;

        return Ok(friendlyResourceToReturn);
    }

    [RequestHeaderMatchesMediaType("Accept", "application/vnd.marvin.hateoas+json", "application/vnd.marvin.author.friendly.hateoas+json")]
    [Produces("application/vnd.marvin.hateoas+json", "application/vnd.marvin.author.friendly.hateoas+json")]
    [HttpGet("{authorId}")]
    public async Task<IActionResult> GetAuthorWithLinks(Guid authorId, string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 400, detail: $"Not all requested data shaping fields exist on the resource: {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo is null)
        {
            return NotFound();
        }

        var links = CreateLinksForAuthor(authorId, fields);

        // friendly author
        var friendlyResourceToReturn = _mapper.Map<AuthorDto>(authorFromRepo).ShapeData(fields) as IDictionary<string, object?>;

        friendlyResourceToReturn.Add("links", links);
        return Ok(friendlyResourceToReturn);
    }

    [RequestHeaderMatchesMediaType("Accept", "application/vnd.marvin.author.friendly+json")]
    [Produces("application/vnd.marvin.author.friendly+json")]
    [HttpGet("{authorId}", Name = "GetAuthor")]
    public async Task<IActionResult> GetFullAuthorWithoutLinks(Guid authorId, string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 400, detail: $"Not all requested data shaping fields exist on the resource: {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo is null)
        {
            return NotFound();
        }

        // friendly author
        var friendlyResourceToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo).ShapeData(fields) as IDictionary<string, object>;

        return Ok(friendlyResourceToReturn);
    }

    [RequestHeaderMatchesMediaType("Accept", "application/vnd.marvin.author.full.hateoas+json")]
    [Produces("application/vnd.marvin.author.full.hateoas+json")]
    [HttpGet("{authorId}")]
    public async Task<IActionResult> GetFullAuthorWithLinks(Guid authorId, string? fields)
    {
        if (!_propertyCheckerService.TypeHasProperties<AuthorDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(HttpContext, statusCode: 400, detail: $"Not all requested data shaping fields exist on the resource: {fields}"));
        }

        // get author from repo
        var authorFromRepo = await _courseLibraryRepository.GetAuthorAsync(authorId);

        if (authorFromRepo is null)
        {
            return NotFound();
        }

        var links = CreateLinksForAuthor(authorId, fields);

        // friendly author
        var friendlyResourceToReturn = _mapper.Map<AuthorFullDto>(authorFromRepo).ShapeData(fields) as IDictionary<string, object?>;

        friendlyResourceToReturn.Add("links", links);
        return Ok(friendlyResourceToReturn);
    }

    private IEnumerable<LinkDto> CreateLinksForAuthor(Guid authorId, string? fields)
    {
        var links = new List<LinkDto>();

        if (string.IsNullOrWhiteSpace(fields))
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetAuthor", new { authorId }),
                Rel = "self",
                Method = "GET"
            });
        }
        else
        {
            links.Add(new LinkDto
            {
                Href = Url.Link("GetAuthor", new { authorId, fields }),
                Rel = "self",
                Method = "GET"
            });
        }

        links.Add(new LinkDto
        {
            Href = Url.Link("CreateCourseForAuthor", new { authorId }),
            Rel = "create_course_for_author",
            Method = "POST"
        });

        links.Add(new LinkDto
        {
            Href = Url.Link("GetCoursesForAuthor", new { authorId }),
            Rel = "get_courses_for_author",
            Method = "GET"
        });

        return links;
    }

    private IEnumerable<LinkDto> CreateLinksForAuthors(AuthorResourceParameter? authorResourceParameter, bool hasNext, bool hasPrevious)
    {
        var links = new List<LinkDto>();

        // self
        links.Add(new LinkDto
        {
           Href = CreateAuthorResourceUri(authorResourceParameter, ResoureUriType.CurrentPage),
           Rel = "self",
           Method = "GET"
        });

        if (hasNext)
        {
            links.Add(new LinkDto
            {
                Href = CreateAuthorResourceUri(authorResourceParameter, ResoureUriType.NextPage),
                Rel = "next_page",
                Method = "GET"
            });
        }

        if (hasPrevious)
        {
            links.Add(new LinkDto
            {
                Href = CreateAuthorResourceUri(authorResourceParameter, ResoureUriType.PreviousPage),
                Rel = "previous_page",
                Method = "GET"
            });
        }

        return links;
    }

    [HttpPost(Name = "CreateAuthorWithDateOfDeath")]
    [RequestHeaderMatchesMediaType("Content-Type", "application/vnd.marvin.authorforcreationwithdateofdeath+json")]
    [Consumes("application/vnd.marvin.authorforcreationwithdateofdeath+json")]
    public async Task<ActionResult<AuthorDto>> CreateAuthorWithDateOfDeath([FromBody] AuthorForCreationWithDateOfDeathDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        // create HATEOAS links
        var links = CreateLinksForAuthor(authorToReturn.Id, null);


        // construct the body as an ExpandoObject
        var body = authorToReturn.ShapeData(null) as IDictionary<string, object?>;
        body.Add("links", links);

        return CreatedAtRoute("GetAuthor",
            new { authorId = body["Id"] },
            body);
    }

    [HttpPost(Name = "CreateAuthor")]
    [RequestHeaderMatchesMediaType("Content-Type", "application/json", "application/vnd.marvin.authorforcreation+json")]
    [Consumes("application/json", "application/vnd.marvin.authorforcreation+json")]  
    public async Task<ActionResult<AuthorDto>> CreateAuthor([FromBody] AuthorForCreationDto author)
    {
        var authorEntity = _mapper.Map<Entities.Author>(author);

        _courseLibraryRepository.AddAuthor(authorEntity);
        await _courseLibraryRepository.SaveAsync();

        var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

        // create HATEOAS links
        var links = CreateLinksForAuthor(authorToReturn.Id, null);


        // construct the body as an ExpandoObject
        var body = authorToReturn.ShapeData(null) as IDictionary<string, object?>;
        body.Add("links", links);

        return CreatedAtRoute("GetAuthor",
            new { authorId = body["Id"] },
            body);
    }

    [HttpOptions]
    public IActionResult GetAuthorsOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,POST,OPTIONS");
        return Ok();
    }
}
