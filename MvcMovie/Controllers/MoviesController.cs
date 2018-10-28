using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MvcMovie.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MvcMovieContext _context;
        private readonly IConfiguration config;

        public MoviesController(MvcMovieContext context, IConfiguration iconfig)
        {
            _context = context;
            config = iconfig;
        }



        // Requires using Microsoft.AspNetCore.Mvc.Rendering;
        public async Task<IActionResult> Index(string movieGenre, string searchString)
        {
            // Use LINQ to get list of genres.
            IQueryable<string> genreQuery = from m in _context.Movie
                                            orderby m.Genre
                                            select m.Genre;

            var movies = from m in _context.Movie
                         select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                movies = movies.Where(s => s.Title.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(movieGenre))
            {
                movies = movies.Where(x => x.Genre == movieGenre);
            }

            var movieGenreVM = new MovieGenreViewModel();
            movieGenreVM.genres = new SelectList(await genreQuery.Distinct().ToListAsync());
            movieGenreVM.movies = await movies.ToListAsync();

            return View(movieGenreVM);
        }

        [HttpPost]
        public string Index(string searchString, bool notUsed)
        {
            return "From [HttpPost]Index: filter on " + searchString;
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            IQueryable<int> reviewQuery = from m in _context.Review
                                             orderby m.MovieID
                                             select m.MovieID;

            return View(movie);
        }

        public async Task<IActionResult> GetFromIMDB(string movietitle, string __RequestVerificationToken)
        {
            if(movietitle == null)
            {
                ModelState.AddModelError(string.Empty, "No movie was entered.");
                return View();
            }
            HttpClient client = new HttpClient();
            movietitle = movietitle.Replace(" ", "+");
            string url = "http://www.omdbapi.com/?t=" + movietitle + "&apikey=" + config.GetValue<string>("APIKey");
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject(data).ToString();
            dynamic omdbMovie = JObject.Parse(json);
            Movie movie = new Movie();
            movie.Title = omdbMovie.Title;
            string genre = omdbMovie.Genre;
            if (genre == null)
            {
                ModelState.AddModelError(string.Empty, "\"" + movietitle + "\" was not found.");
                return View();
            }
            if (genre.Contains(","))
            {
                string[] genres = genre.Split(",");
                movie.Genre = genres[0];
            }
            else
            {
                movie.Genre = omdbMovie.Genre;
            }
            string rating = omdbMovie.Rated;
            if (rating == "NOT RATED" || rating == "UNRATED")
            {
                rating = "NR";
                movie.Rating = rating;
            }
            else
            {
                movie.Rating = omdbMovie.Rated;
            }
            movie.ReleaseDate = omdbMovie.Released;
            movie.Poster = omdbMovie.Poster;
            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,ReleaseDate,Genre,Price,Rating,Poster")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                Movie existingMovie = await _context.Movie.SingleOrDefaultAsync(m => m.Title == movie.Title);
                if (existingMovie == null)
                {
                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "" + movie.Title + " is already in the database.");
                    return View(movie);
                }
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,ReleaseDate,Genre,Price,Rating")] Movie movie)
        {
            if (id != movie.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.ID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.ID == id);
        }
    }
}
