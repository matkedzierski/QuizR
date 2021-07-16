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

        // GET: Room/Play/4
        public ActionResult Play(int? id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            
            if (id == null)
                return RedirectToAction("Index");
            else //jesli podano id pokoju
            {
                Debug.WriteLine("Finding: " + id);
                var room = db.Rooms.Include(r => r.Owner).FirstOrDefault(r => r.ID == id); //znajdz z bazy
                if (room == null) //jesli w bazie ma, to przejdz do tworzenia
                {
                    return RedirectToAction("Create");
                }
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




        // GET: Room/Edit/4
        public ActionResult Edit(int id)
        {
            Debug.WriteLine(id);
            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            var room = db.Rooms
                .Include(r => r.Owner)
                .SingleOrDefault(r => r.ID == id);
            if (room == null || !room.Owner.Equals(user)) return View("Error");


            var list = db.QuestionSets.Include(qs => qs.Owner).Where(qs => qs.Owner.Id == user.Id).ToList();
            ViewBag.MySets = list;

            return View(room);
        }

        // POST: Room/Edit/4
        [HttpPost]
        public ActionResult Edit(QuizRoom room, string SetID)
        {
            Debug.WriteLine("new: " + room.Name);
            var user = UserManager.FindById(User.Identity.GetUserId());

            var list = db.QuestionSets.Include(qs => qs.Owner).Where(qs => qs.Owner.Id == user.Id).ToList();
            ViewBag.MySets = list;

            //jesli wybrano zestaw
            if (SetID != null)
                room.Set = db.QuestionSets.Find(int.Parse(SetID));
            room.Owner = user;

            ModelState.Clear();
            if (!TryValidateModel(room))
            {
                return View(room);
            }

            var dbRoom = db.Rooms.Find(room.ID);
            db.Entry(dbRoom).CurrentValues.SetValues(room);
            db.SaveChanges();

            return RedirectToAction("Index");
        }





        // GET: Room/Delete/5
        public ActionResult Delete(int id)
        {
            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            var room = db.Rooms
                .Include(r => r.Owner)
                .SingleOrDefault(r => r.ID == id);

            if (room == null || !room.Owner.Equals(user)) return View("Error");

            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection) //formcollection żeby nie było dwóch takich samych metod
        {
            try
            {
                //sprawdzanie czy jest wlascicielem
                var user = UserManager.FindById(User.Identity.GetUserId());
                var room = db.Rooms
                    .Include(r => r.Owner)
                    .SingleOrDefault(r => r.ID == id);

                if (room == null || !room.Owner.Equals(user)) return View("Error");

                //usuwanie
                room.Users.Clear();
                db.Rooms.Remove(room);
                db.SaveChanges();
            }
            catch
            {
                Debug.WriteLine("Nie udalo sie usunac!");
            }
            return RedirectToAction("Index");
        }
    }



}