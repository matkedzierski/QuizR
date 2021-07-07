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
            
            return View();
        }

        // POST: Room/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                var user = UserManager.FindById(User.Identity.GetUserId());

                string id = collection["ID"];
                int setID = int.Parse(collection["Set"]);
                var set = db.QuestionSets.Find(setID);


                var qr = new QuizRoom() { ID = id, Owner = user, Set = set };
                db.Rooms.Add(qr);
                db.SaveChanges();

                return RedirectToAction("Play", new { id = id });
            }
            catch
            {
                return View();
            }
        }
    }



}