using KeinishkataKushta.Infrastructure;
using KeinishkataKushta.Models;
using Microsoft.AspNetCore.Mvc;

namespace KeinishkataKushta.Controllers
{
    public class PlacesController : Controller
    {
        public IActionResult Index()
        {
            var en = SiteText.Lang(HttpContext) == "en";

            var places = new List<NearbyPlaceViewModel>
            {
                new()
                {
                    Name = en ? "Covered Bridge, Lovech" : "Покритият мост, Ловеч",
                    Category = en ? "Landmark" : "Забележителност",
                    Distance = en ? "Lovech" : "Ловеч",
                    Description = en
                        ? "The symbol of Lovech, connecting the old Varosha quarter with the newer part of town over the Osam River."
                        : "Символът на Ловеч, който свързва стария квартал Вароша с новата част на града над река Осъм.",
                    VisitUrl = "https://bulgariatravel.org/the-covered-bridge-lovech/",
                    MapUrl = "https://www.google.com/maps/search/?api=1&query=Covered+Bridge+Lovech"
                },
                new()
                {
                    Name = en ? "Varosha Old Town" : "Старият квартал Вароша",
                    Category = en ? "Walk" : "Разходка",
                    Distance = en ? "Lovech" : "Ловеч",
                    Description = en
                        ? "A quiet old-town area with traditional houses, small streets, and easy walks around the historic center."
                        : "Спокоен старинен квартал с възрожденски къщи, малки улици и приятни разходки около историческия център.",
                    VisitUrl = "https://www.lovech.bg/",
                    MapUrl = "https://www.google.com/maps/search/?api=1&query=Varosha+Lovech"
                },
                new()
                {
                    Name = en ? "Devetashka Cave" : "Деветашка пещера",
                    Category = en ? "Nature" : "Природа",
                    Distance = en ? "Near Devetaki" : "край Деветаки",
                    Description = en
                        ? "One of Bulgaria's most impressive caves, known for its huge open halls, natural light, and bat colonies."
                        : "Една от най-впечатляващите пещери в България, известна с огромните си зали, естествената светлина и колониите прилепи.",
                    VisitUrl = "https://kilometri.bg/en/view/devetashka-cave",
                    MapUrl = "https://www.google.com/maps/search/?api=1&query=Devetashka+Cave"
                },
                new()
                {
                    Name = en ? "Krushuna Waterfalls" : "Крушунски водопади",
                    Category = en ? "Nature" : "Природа",
                    Distance = en ? "Near Krushuna" : "край Крушуна",
                    Description = en
                        ? "A green waterfall walk with blue pools, limestone terraces, and an easy eco-trail."
                        : "Зелена разходка с водопади, синкави басейни, варовикови тераси и лека екопътека.",
                    VisitUrl = "https://www.waterfallsbg.info/en/krushunski-waterfalls",
                    MapUrl = "https://www.google.com/maps/search/?api=1&query=Krushuna+Waterfalls"
                },
                new()
                {
                    Name = en ? "Kakrina Inn" : "Къкринското ханче",
                    Category = en ? "History" : "История",
                    Distance = en ? "Near Kakrina" : "край Къкрина",
                    Description = en
                        ? "A historic place connected with Vasil Levski, suitable for a short cultural stop while exploring the region."
                        : "Историческо място, свързано с Васил Левски, подходящо за кратка културна спирка в района.",
                    VisitUrl = "https://www.google.com/search?q=Kakrina+Inn",
                    MapUrl = "https://www.google.com/maps/search/?api=1&query=Kakrina+Inn"
                },
                new()
                {
                    Name = en ? "Troyan Monastery" : "Троянски манастир",
                    Category = en ? "Day trip" : "Еднодневна разходка",
                    Distance = en ? "Troyan region" : "Троянски район",
                    Description = en
                        ? "A larger day-trip option with mountain scenery, monastery architecture, and traditional craft villages nearby."
                        : "По-дълга разходка с планински пейзажи, манастирска архитектура и близки села с традиционни занаяти.",
                    VisitUrl = "https://www.google.com/search?q=Troyan+Monastery",
                    MapUrl = "https://www.google.com/maps/search/?api=1&query=Troyan+Monastery"
                }
            };

            return View(places);
        }
    }
}
