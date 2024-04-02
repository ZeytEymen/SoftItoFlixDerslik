using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using SoftITOFlix.Data;
using SoftITOFlix.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace SoftITOFlix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        public class ResultStruct
        {
            public string Title { get; set; } = "";
            public short Year { get; set; }
            public string ImdbId { get; set; } = "";
            public string Type { get; set; } = "";
            public string Poster { get; set; } = "";
        }
        public struct IMDBNameResult
        {
            bool Success { get; set; }
            //List<ResultStruct> Result { get; set; }
        }
        private readonly SoftITOFlixContext _context;

        public CategoriesController(SoftITOFlixContext context)
        {
            _context = context;
        }

        [HttpGet("IMDB")]
        public IMDBNameResult IMDB(string title)
        {
            
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("authorization", "apikey 1ESZpNKxD3uKoCpGTVIItJ:3JXkM32N2EDyLaOj5Q00K9");
            
            HttpResponseMessage httpResponseMessage = httpClient.GetAsync("https://api.collectapi.com/imdb/imdbSearchByName?query=" + title).Result;
            string iMDBNameResult = httpResponseMessage.Content.ReadAsStringAsync().Result;
            JsonSerializer jsonSerializer = new JsonSerializer();
            object? nameResult = JsonConvert.DeserializeObject(iMDBNameResult);
            
            return (IMDBNameResult)nameResult!;
        }

        // GET: api/Categories
        [HttpGet]
        public ActionResult<List<Category>> GetCategories()
        {
            return _context.Categories.AsNoTracking().ToList();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<Category> GetCategory(short id)
        {
            Category? category = _context.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "ContentAdmin")]
        public void PutCategory(Category category)
        {
            _context.Categories.Update(category);

            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
            }
        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "ContentAdmin")]
        public short PostCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();

            return category.Id;
        }
    }
}
