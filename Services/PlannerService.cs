using System.Collections.Generic;
using System.Linq;
using TourGuideSmart.Models;

namespace TourGuideSmart.Services
{
    public class PlannerService
    {
        public List<Tour> SuggestTour(List<Tour> tours, int budget)
        {
            return tours.Where(t => t.Price <= budget / 2).Take(3).ToList();
        }
    }
}
