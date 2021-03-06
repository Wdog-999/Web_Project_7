﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Models;

namespace MvcMovie.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly MvcMovieContext _context;

        public ReviewsController(MvcMovieContext context)
        {
            _context = context;
        }

        // GET: Reviews
        public async Task<IActionResult> Index(String sortOrder)
        {
            ViewData["NameSortParm"] = sortOrder == "Film" ? "Film_desc" : "Film";
            ViewData["MovieSortParm"] = sortOrder == "Reviewer" ? "Reviewer_desc" : "Reviewer";

            var reviews = from m in _context.Review
                         select m;

            switch (sortOrder)
            {
                case "Film_desc":
                    reviews = reviews.OrderByDescending(s => s.Title);
                    break;
                case "Film":
                    reviews = reviews.OrderBy(s => s.Title);
                    break;
                case "Reviewer_desc":
                    reviews = reviews.OrderByDescending(s => s.Name);
                    break;
                case "Reviewer":
                    reviews = reviews.OrderBy(s => s.Name);
                    break;
            }

            return View(await reviews.ToListAsync());
        }



        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .FirstOrDefaultAsync(m => m.ID == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        public class MovieRedirect
        {
            public string ID { get; set; }
        }

        // GET: Reviews/Create
        public IActionResult Create(string movietitle, string movieID)
        {
            Review review = new Review();
            review.MovieID = Int32.Parse(movieID);
            review.Title = movietitle;
            return View(review);
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID, MovieID,Title,Name,Comment")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                var ID = new MovieRedirect();
                ID.ID = review.MovieID.ToString();
                return RedirectToAction("Details", "Movies", ID);
            }
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID, MovieID,Title,Name,Comment")] Review review)
        {
            if (id != review.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                var ID = new MovieRedirect();
                ID.ID = review.MovieID.ToString();
                return RedirectToAction("Details", "Movies", ID);
            }
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .FirstOrDefaultAsync(m => m.ID == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Review.FindAsync(id);
            var ID = new MovieRedirect();
            ID.ID = review.MovieID.ToString();
            _context.Review.Remove(review);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Movies", ID);
        }

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.ID == id);
        }
    }
}
