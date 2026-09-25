using Microsoft.AspNetCore.Http;

namespace KeinishkataKushta.Infrastructure
{
    public static class SiteText
    {
        private const string CookieName = "kk_lang";

        private static readonly Dictionary<string, (string Bg, string En)> Texts = new()
        {
            ["SocialProfiles"] = ("Социални мрежи", "Social media"),
            ["OpensNewTab"] = ("отваря се в нов раздел", "opens in a new tab"),
            ["Availability"] = ("Свободни дати", "Availability"),
            ["AvailabilityLoading"] = ("Зареждане...", "Loading..."),
            ["AvailabilityLoadError"] = ("Не успяхме да заредим месеца. Опитайте отново.", "We couldn't load this month. Please try again."),
            ["AvailabilityIntro"] = ("Изберете свободни нощувки за вашата компания.", "Find available nights for your group."),
            ["AvailabilityDisclaimer"] = ("Датите подлежат на потвърждение. За резервация се свържете с нас.", "Dates are subject to confirmation. Please contact us to book."),
            ["AvailabilityNights"] = ("Показани са нощувките. Датата на отпътуване не е включена.", "Availability is shown per night. The departure date is not included."),
            ["AvailabilityExclusive"] = ("По време на престоя ви не настаняваме други гости. Броят на предоставените стаи се уточнява при резервацията.", "We do not accommodate other guests during your stay. The rooms included are agreed when booking."),
            ["WholeHouse"] = ("Цялата къща", "Whole house"),
            ["AvailabilityRoomsUsed"] = ("Предоставени стаи (само за администратора)", "Rooms included (admin only)"),
            ["AvailabilityAvailable"] = ("Свободно", "Available"),
            ["AvailabilityOccupied"] = ("Заето", "Occupied"),
            ["AvailabilityUnavailable"] = ("Недостъпно", "Unavailable"),
            ["AvailabilityPast"] = ("Минала дата", "Past date"),
            ["AvailabilityToday"] = ("Днес", "Today"),
            ["AvailabilityPrevious"] = ("Предишен месец", "Previous month"),
            ["AvailabilityNext"] = ("Следващ месец", "Next month"),
            ["AvailabilityEnquire"] = ("Изпратете запитване", "Send an enquiry"),
            ["AvailabilityAdd"] = ("Добави период", "Add dates"),
            ["AvailabilityEdit"] = ("Редактирай", "Edit"),
            ["AvailabilityDelete"] = ("Премахни", "Remove"),
            ["AvailabilitySave"] = ("Запази", "Save"),
            ["AvailabilityCancel"] = ("Отказ", "Cancel"),
            ["AvailabilityArrival"] = ("Настаняване / начало", "Arrival / start"),
            ["AvailabilityDeparture"] = ("Напускане / край", "Departure / end"),
            ["AvailabilityKind"] = ("Причина", "Reason"),
            ["AvailabilityReservation"] = ("Резервация", "Reservation"),
            ["AvailabilityMaintenance"] = ("Поддръжка / затворено", "Maintenance / closed"),
            ["AvailabilityNotes"] = ("Лична бележка (само за администратора)", "Private note (admin only)"),
            ["AvailabilityHistory"] = ("Всички периоди", "All entries"),
            ["AvailabilityUpcoming"] = ("Текущи и предстоящи", "Current and upcoming"),
            ["AvailabilityNoEntries"] = ("Няма добавени периоди.", "No entries yet."),
            ["AvailabilitySaved"] = ("Свободните дати са обновени.", "Availability updated."),
            ["AvailabilityDeleteConfirm"] = ("Да премахнем ли този период? Датите ще станат свободни, ако няма други ограничения.", "Remove this entry? These dates will become available if no other restrictions apply."),
            ["AvailabilityAdminNote"] = ("Всяка резервация или затваряне блокира цялата къща, независимо от броя предоставени стаи. Добавете и резервациите, получени по телефон или от други платформи.", "Every reservation or closure blocks the whole property, regardless of the rooms included. Also enter reservations received by phone or through other platforms."),
            ["AvailabilityInvalidDates"] = ("Крайната дата трябва да е след началната.", "Departure must be after arrival."),
            ["AvailabilityConflict"] = ("Периодът се застъпва с друга резервация или затваряне. Настаняваме само една компания наведнъж.", "These dates overlap another reservation or closure. Only one group can stay at a time."),
            ["AvailabilityChanged"] = ("Записът е променен или изтрит междувременно. Отворете отново списъка и редактирайте актуалния запис.", "This entry was changed or removed. Return to the list and reopen the current entry."),
            ["AvailabilityRetry"] = ("Данните са променени междувременно. Проверете календара и опитайте отново.", "Availability changed while saving. Check the calendar and try again."),
            ["QuickContacts"] = ("Бързи контакти", "Quick contacts"),
            ["CallUs"] = ("Обадете ни се", "Call us"),
            ["WriteToUs"] = ("Пишете ни", "Write to us"),
            ["VisitFacebook"] = ("Последвайте ни", "Follow us"),
            ["FindUs"] = ("Намерете ни", "Find us"),
            ["Facebook"] = ("Facebook", "Facebook"),
            ["Address"] = ("Адрес", "Address"),
            ["HouseAddress"] = ("ул. Захари Стоянов 30, 5787, с. Дъбен", "30 Zahari Stoyanov St., 5787 Daben"),
            ["Gallery"] = ("Галерия", "Gallery"),
            ["GalleryLabel"] = ("Къщата и дворът", "House and grounds"),
            ["GalleryTitle"] = ("Разгледайте Кейнишката къща", "Explore Keinishkata House"),
            ["GalleryIntro"] = ("Вижте двора, механата, барбекюто и местата за спокойни моменти на открито.", "See the courtyard, tavern, barbecue area, and the spaces made for relaxing outdoors."),
            ["GalleryEmpty"] = ("Скоро ще добавим снимки на къщата и общите пространства.", "Photos of the house and shared spaces are coming soon."),
            ["PropertyPhoto"] = ("Кейнишката къща", "Keinishkata House"),
            ["OpenImage"] = ("Отвори снимката", "Open image"),
            ["CloseGallery"] = ("Затвори галерията", "Close gallery"),
            ["PreviousImage"] = ("Предишна снимка", "Previous image"),
            ["NextImage"] = ("Следваща снимка", "Next image"),
            ["Image"] = ("Снимка", "Image"),
            ["PublishedRooms"] = ("стаи за избор", "rooms to choose from"),
            ["GuestsPerRoom"] = ("гости в стая", "guests per room"),
            ["PricesFrom"] = ("цени от", "prices from"),
            ["WhyStayLabel"] = ("Вашият престой", "Your stay"), 
            ["WhyStayTitle"] = ("Всичко важно за спокойна почивка", "Everything that matters for a restful stay"),
            ["WhyStayCopy"] = ("Лесно планиране, лично отношение и удобна отправна точка за разходки в района.", "Easy planning, personal attention, and a comfortable base for exploring the region."),
            ["QuietStayTitle"] = ("Тишина и природа", "Peace and nature"),
            ["QuietStayCopy"] = ("Място за бавни сутрини, чист въздух и истинска почивка далеч от градския шум.", "A place for slow mornings, fresh air, and genuine rest away from the noise of the city."),
            ["ComfortStayTitle"] = ("Домашен уют", "Comfort like home"),
            ["ComfortStayCopy"] = ("Уютни стаи и топла атмосфера, създадени за спокойно време с близките ви.", "Cozy rooms and a warm atmosphere made for unhurried time with the people close to you."),
            ["DirectStayTitle"] = ("Директна връзка", "Direct contact"),
            ["DirectStayCopy"] = ("Пишете ни за свободни дати и въпроси и ще получите личен отговор от нас.", "Write to us about available dates and questions and receive a personal reply from us."),
            ["LocalGuideLabel"] = ("Открийте района", "Discover the region"),
            ["LocalGuideTitle"] = ("Един престой, много места за преживяване", "One stay, many places to experience"),
            ["LocalGuideCopy"] = ("От стария Ловеч до пещери, водопади и планински маршрути — подберете разходка според вашето настроение.", "From historic Lovech to caves, waterfalls, and mountain routes, choose a day out that matches your mood."),
            ["ExplorePlaces"] = ("Разгледайте забележителностите", "Explore nearby places"),
            ["HistoricLovech"] = ("Исторически Ловеч", "Historic Lovech"),
            ["DevetashkaCave"] = ("Деветашка пещера", "Devetashka Cave"),
            ["KrushunaWaterfalls"] = ("Крушунски водопади", "Krushuna Waterfalls"),
            ["Brand"] = ("Кейнишката Къща", "Keinishkata House"),
            ["Home"] = ("Начало", "Home"),
            ["Rooms"] = ("Стаи", "Rooms"),
            ["NearbyPlaces"] = ("Забележителности", "Nearby Places"),
            ["Contact"] = ("Контакти", "Contact"),
            ["Admin"] = ("Админ", "Admin"),
            ["Reserve"] = ("Резервирай", "Book now"),
            ["ViewRooms"] = ("Вижте стаите", "View rooms"),
            ["ContactUs"] = ("Свържете се с нас", "Contact us"),
            ["LearnMore"] = ("Научете повече", "Learn more"),
            ["OpenMap"] = ("Отвори карта", "Open map"),
            ["AskUs"] = ("Попитайте ни", "Ask us"),
            ["SendMessage"] = ("Изпрати съобщение", "Send message"),
            ["BackToRooms"] = ("Обратно към стаите", "Back to rooms"),
            ["HeroTitle"] = ("Добре дошли в Кейнишката Къща", "Welcome to Keinishkata House"),
            ["HeroCopy"] = ("Уют, спокойствие и красива природа за вашата почивка.", "Comfort, calm, and beautiful nature for your stay."),
            ["AboutLabel"] = ("За нас", "About us"),
            ["AboutTitle"] = ("Български уют и спокойствие близо до природата", "Bulgarian comfort and calm close to nature"),
            ["AboutCopyOne"] = ("Кейнишката Къща е място за бавни сутрини, чист въздух и истинска почивка. Създаваме усещане за дом, където всеки детайл е подбран с грижа.", "Keinishkata House is a place for slow mornings, fresh air, and true rest. We create the feeling of home, where every detail is chosen with care."),
            ["AboutCopyTwo"] = ("Тук гостите могат да се насладят на уютни стаи, тишина и удобна отправна точка за разходки из региона.", "Here guests can enjoy cozy rooms, quiet surroundings, and a comfortable starting point for exploring the region."),
            ["FeaturedRooms"] = ("Нашите стаи", "Our rooms"),
            ["FeaturedRoomsCopy"] = ("Изберете уютна стая за вашата почивка. Всяка стая съчетава традиционен български уют с модерни удобства.", "Choose a cozy room for your stay. Each room combines traditional Bulgarian warmth with modern comfort."),
            ["AllRooms"] = ("Всички стаи", "All rooms"),
            ["PlanStayTitle"] = ("Планирайте вашата почивка", "Plan your stay"),
            ["PlanStayCopy"] = ("Пишете ни с предпочитани дати и въпроси. Ще ви отговорим с наличност и подробности.", "Send us your preferred dates and questions. We will reply with availability and details."),
            ["RoomsTitle"] = ("Нашите стаи", "Our rooms"),
            ["RoomsIntro"] = ("Разгледайте публикуваните стаи и отворете детайлите, за да видите снимки и информация.", "Browse our published rooms and open the details to see photos and information."),
            ["NoRooms"] = ("Все още няма публикувани стаи.", "No published rooms yet."),
            ["Guests"] = ("гости", "guests"),
            ["UpTo"] = ("До", "Up to"),
            ["Night"] = ("нощувка", "night"),
            ["Details"] = ("Виж детайли", "View details"),
            ["RoomInterested"] = ("Интересувате се от тази стая?", "Interested in this room?"),
            ["RoomInterestedCopy"] = ("Изпратете кратко запитване с предпочитани дати и ще ви отговорим с наличност.", "Send a short inquiry with your preferred dates and we will reply with availability."),
            ["NearbyTitle"] = ("Забележителности наблизо", "Nearby places"),
            ["NearbyIntro"] = ("Открийте красиви места, природни забележителности и интересни маршрути в района.", "Discover beautiful places, natural landmarks, and interesting routes in the area."),
            ["NearbyPlanning"] = ("Планирате престой?", "Planning your stay?"),
            ["NearbyPlanningCopy"] = ("Кажете ни какъв тип разходки харесвате и ще ви насочим към подходящи места наблизо.", "Tell us what kind of trips you enjoy and we can point you toward nearby places."),
            ["ContactTitle"] = ("Свържете се с нас", "Contact us"),
            ["ContactIntro"] = ("За резервации и въпроси ни изпратете съобщение или се свържете с нас по телефон.", "For reservations and questions, send us a message or contact us by phone."),
            ["InquiryTitle"] = ("Изпратете запитване", "Send an inquiry"),
            ["Name"] = ("Име и фамилия", "Full name"),
            ["Email"] = ("Имейл", "Email"),
            ["Phone"] = ("Телефон", "Phone"),
            ["Message"] = ("Съобщение", "Message"),
            ["MessagePlaceholder"] = ("Как можем да ви помогнем?", "How can we help?"),
            ["ContactSuccess"] = ("Благодарим ви. Съобщението е изпратено.", "Thank you. Your message has been sent."),
            ["FooterCopy"] = ("Планина, спокойствие, уют.", "Nature, calm, comfort."),
            ["Copyright"] = ("Всички права запазени.", "All rights reserved.")
        };

        public static string Lang(HttpContext context)
        {
            var queryLang = context.Request.Query["lang"].FirstOrDefault();
            var lang = string.IsNullOrWhiteSpace(queryLang)
                ? context.Request.Cookies[CookieName]
                : queryLang;

            return string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "bg";
        }

        public static string T(HttpContext context, string key)
        {
            if (!Texts.TryGetValue(key, out var value))
            {
                return key;
            }

            return Lang(context) == "en" ? value.En : value.Bg;
        }
    }
}
