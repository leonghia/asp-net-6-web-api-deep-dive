using AutoMapper;
using CourseLibrary.API.Entities;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using CourseLibrary.API.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CourseLibrary.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorCollectionController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public AuthorCollectionController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
        }

        [HttpGet("({authorIds})", Name = "GetAuthorCollection")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))][FromRoute] Guid[] authorIds)
        {
            var authorEntities = await _courseLibraryRepository.GetAuthorsAsync(authorIds);

            // Do we have all requested authors?
            if (authorIds.Count() != authorEntities.Count())
                return NotFound();

            // Map
            var authorsToReturn = _mapper.Map<AuthorDto[]>(authorEntities);

            return Ok(authorsToReturn);
        }


        [HttpPost]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> CreateAuthorCollection([FromBody] AuthorForCreationDto[] authorCollection)
        {
            var authorEntities = _mapper.Map<IEnumerable<Author>>(authorCollection);
            foreach (var author in authorEntities)
            {
                _courseLibraryRepository.AddAuthor(author);
            }
            await _courseLibraryRepository.SaveAsync();

            var authorsCollectionToReturn = _mapper.Map<AuthorDto[]>(authorEntities);
            var authorIdsAsString = string.Join(",", authorsCollectionToReturn.Select(author => author.Id));

            return CreatedAtRoute("GetAuthorCollection", new { authorIds = authorIdsAsString }, authorsCollectionToReturn);
        }
    }
}
