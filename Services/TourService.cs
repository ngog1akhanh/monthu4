using System.Collections.Generic;
using TourGuideSmart.Models;
using TourGuideSmart.Data;

namespace TourGuideSmart.Services
{
    public class TourService
    {
        // Return mock data from a central place to make testing easier
        public List<Tour> GetTours()
        {
            return MockData.GetMockTours();
        }
    }
}
