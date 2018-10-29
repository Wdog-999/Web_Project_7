using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace MvcMovie.Models
{
    public class MovieReviewViewModel
    {
        public Movie movie;
        public IQueryable<Review> reviews;
    }
}