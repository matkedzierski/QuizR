using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using QuizR.Models;
using System.Data.Entity;

namespace QuizR.Controllers
{
    [Authorize]
    public class QuestionSetsController : Controller
    {

        public ApplicationDbContext db { get; set; }
        public UserManager<ApplicationUser> UserManager { get; set; }

        public QuestionSetsController()
        {
            db = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }

        // GET: QuestionSets
        public ActionResult Index()
        {
            //zestawy obecnego uzytkownika
            var user = UserManager.FindById(User.Identity.GetUserId());
            var sets = db.QuestionSets.Where(qs => qs.Owner.Id == user.Id).ToList(); 
            return View(sets);
        }

        // GET: QuestionSets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: QuestionSets/Create
        [HttpPost]
        public ActionResult Create(QuestionSet set)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());

            //var set = new QuestionSet() { Owner = user, Name = name };
            if(ModelState.IsValid)
            {
                set.Owner = user;
                db.QuestionSets.Add(set);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(set);
        }

        // GET: QuestionSets/Edit/5
        public ActionResult Edit(int id)
        {
            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            var set = db.QuestionSets
                .Include(qs => qs.Owner)
                .SingleOrDefault(qs => qs.ID == id);
            if (set == null || !set.Owner.Equals(user)) return View("Error");

            return View(set);
        }

        // POST: QuestionSets/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                //sprawdzanie czy jest wlascicielem
                var user = UserManager.FindById(User.Identity.GetUserId());
                var set = db.QuestionSets
                    .Include(qs => qs.Owner)
                    .SingleOrDefault(qs => qs.ID == id);
                if (set == null || !set.Owner.Equals(user)) return View("Error");

                //zmiana nazwy
                var newName = collection["Name"];
                set.Name = newName;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: QuestionSets/Delete/5
        public ActionResult Delete(int id)
        {
            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            var set = db.QuestionSets
                .Include(qs => qs.Owner)
                .SingleOrDefault(qs => qs.ID == id);
            if (set == null || !set.Owner.Equals(user)) return View("Error");
            return View(set);
        }

        // POST: QuestionSets/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                //sprawdzanie czy jest wlascicielem
                var user = UserManager.FindById(User.Identity.GetUserId());
                var set = db.QuestionSets
                    .Include(qs => qs.Owner)
                    .SingleOrDefault(qs => qs.ID == id);
                if (set == null || !set.Owner.Equals(user)) return View("Error");

                //usuwanie
                db.QuestionSets.Remove(set);
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