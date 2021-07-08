using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.SignalR;
using QuizR.Models;
using System.Data.Entity;

namespace QuizR.Controllers
{
    [System.Web.Mvc.Authorize]
    public class RoomController : Controller
    {
        public ApplicationDbContext db { get; set; }
        public UserManager<ApplicationUser> UserManager { get; set; }
        public RoomController()
        {
            db = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        // GET: Room
        public ActionResult Index()
        {
            // wczytaj wszystkie pokoje z bazy
            var rooms = db.Rooms.Include(r => r.Owner).ToList();
            return View(rooms);
        }

        // GET: Room/Play?RoomID=xxx
        public ActionResult Play(string id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            
            if (id == null)
                return RedirectToAction("Index", "Home");
            else //jesli podano id pokoju
            {
                Debug.WriteLine("Finding: " + id);
                var room = db.Rooms.Include(r => r.Owner).FirstOrDefault(r => r.ID == id); //znajdz z bazy
                if (room == null) //jesli w bazie nie ma
                    return View("Error");
                return View(room);
            }
        }


        // GET: Room/Create
        public ActionResult Create()
        {
            var userID = User.Identity.GetUserId();
            var list = db.QuestionSets.Include(qs => qs.Owner).Where(qs => qs.Owner.Id == userID).ToList();
            ViewBag.MySets = list;

            return View();
        }

        // POST: Room/Create
        [HttpPost]
        public ActionResult Create(QuizRoom room, string SetID)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            
            var list = db.QuestionSets.Include(qs => qs.Owner).Where(qs => qs.Owner.Id == user.Id).ToList();
            ViewBag.MySets = list;

            if (db.Rooms.Find(room.ID) != null)
            {
                ModelState.Clear();
                ModelState.AddModelError("ID", "Pokój o takiej nazwie już istnieje!");
                return View(room);
            }

            if(SetID != null)
                room.Set = db.QuestionSets.Find(int.Parse(SetID));
            room.Owner = user;

            ModelState.Clear();
            if (!TryValidateModel(room))
            {
                return View(room);
            }

            
            db.Rooms.Add(room);
            db.SaveChanges();

            return RedirectToAction("Play", new { id = room.ID });
        }
    }



}