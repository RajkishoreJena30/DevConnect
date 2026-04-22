using DevConnect.Data;
using DevConnect.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevConnect.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        //static private List<Books> books = new List<Books>
        //{
        //    new Books
        //    {
        //        Id = 1,
        //        Title = "Title",
        //        Author = "Author",
        //        YearOfPublished = 2021
        //    },
        //    new Books
        //    {
        //        Id = 2,
        //        Title = "Title2",
        //        Author = "Author2",
        //        YearOfPublished = 2024
        //    },
        //    new Books
        //    {
        //        Id = 3,
        //        Title = "Title3",
        //        Author= "Author3",
        //        YearOfPublished = 2025
        //    }
        //};

        private readonly FirstAPIContext _firstAPIContext;

        public BooksController(FirstAPIContext firstAPIContext)
        {
            _firstAPIContext = firstAPIContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<Books>>> GetBooks()
        {
            return Ok( await _firstAPIContext.Books.ToListAsync());
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<Books>> GetBookById(int id)
        {
            var book = await _firstAPIContext.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        public async  Task<ActionResult<Books>> AddBook (Books newBook)
        {
            if (newBook == null)
                return BadRequest();

            _firstAPIContext.Books.Add(newBook);
            await _firstAPIContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Books updatedBook)
        {
            //var book = books.FirstOrDefault(x => x.Id == id);

            var book  = await _firstAPIContext.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            //book.Id = updatedBook.Id;
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.YearOfPublished = updatedBook.YearOfPublished;

            await _firstAPIContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]

        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _firstAPIContext.Books.FindAsync(id);
            if(book == null)
                return NotFound();

            _firstAPIContext.Books.Remove(book);
            await _firstAPIContext.SaveChangesAsync();

            return NoContent();
        }

        //[HttpGet]
        //public ActionResult<List<Books>> GetBooks()
        //{
        //    return Ok(books);
        //}

        //[HttpGet("{id}")]

        //public ActionResult<Books> GetBookById(int id)
        //{
        //    var book = books.FirstOrDefault(x => x.Id == id);

        //    if(book == null)
        //        return NotFound();

        //    return Ok(book);
        //}

        //[HttpPost]

        //public ActionResult<Books> AddBook (Books newBook)
        //{
        //    if (newBook == null)
        //        return BadRequest();
        //    books.Add(newBook);
        //    return CreatedAtAction(nameof(GetBookById), new { id = newBook.Id }, newBook);
        //}

        //[HttpPut("{id}")]
        //public IActionResult UpdateBook(int id,Books updatedBook)
        //{
        //    var book = books.FirstOrDefault(x =>x.Id == id);
        //    if (book == null)
        //        return NotFound();

        //    book.Id = updatedBook.Id;
        //    book.Title = updatedBook.Title;
        //    book.Author = updatedBook.Author;
        //    book.YearOfPublished = updatedBook.YearOfPublished;

        //    return NoContent();
        //}
    }
}
//INGBTCPIC5NB136\SQLEXPRESS