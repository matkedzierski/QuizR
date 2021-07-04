using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using QuizR.Models;
using System.Data.Entity;

namespace QuizR.Controllers
{
    public class QuestionsController : Controller
    {
        public ApplicationDbContext db { get; set; }
        public UserManager<ApplicationUser> UserManager { get; set; }

        public QuestionsController()
        {
            db = new ApplicationDbContext();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
        }


        // GET: Questions/5 (id zestawu)
        [Authorize]
        public ActionResult Manage(int id)
        {
            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            var set = db.QuestionSets.Find(id);
            if (set == null || !set.Owner.Equals(user)) return View("Error");

            return View(set);
        }

        // GET: Questions/Create
        public ActionResult Create(int setID)
        {
            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            var set = db.QuestionSets.Find(setID);
            if (set == null || !set.Owner.Equals(user)) return View("Error");
            ViewBag.SetID = setID;
            return View();
        }

        // POST: Questions/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                //parsowanie danych z formularza
                string content = collection["Content"];
                string[] ans = new string[4];
                ans[0] = collection["Aans"];
                ans[1] = collection["Bans"];
                ans[2] = collection["Cans"];
                ans[3] = collection["Dans"];
                string correct = collection["Correct"];
                var setID = int.Parse(collection["setID"]);

                var set = db.QuestionSets.Find(setID);

                //sprawdzanie czy jest wlascicielem
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (set == null || !set.Owner.Equals(user)) return View("Error");


                var q = new Question() { Owner = user, Set = set, Content = content, Aans = ans[0], Bans = ans[1], Cans = ans[2], Dans = ans[3], Correct = correct };
                set?.Questions.Add(q);
                db.SaveChanges();

                return RedirectToAction("Manage", new { id = setID });
            }
            catch
            {
                return View();
            }
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int id)
        {
            var q = db.Questions
                    .Include(r => r.Owner)
                    .Include(r => r.Set)
                    .SingleOrDefault(r => r.ID == id);

            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (q == null || !q.Owner.Equals(user)) return View("Error");

            return View(q);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                var q = db.Questions
                    .Include(r => r.Owner)
                    .Include(r => r.Set)
                    .SingleOrDefault(r => r.ID == id);

                //sprawdzanie czy jest wlascicielem
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (q == null || !q.Owner.Equals(user)) return View("Error");

                q.Content = collection["Content"];
                q.Aans = collection["Aans"];
                q.Bans = collection["Bans"];
                q.Cans = collection["Cans"];
                q.Dans = collection["Dans"];
                q.Correct = collection["Correct"];

                db.SaveChanges();
                return RedirectToAction("Manage", new { id = q.Set.ID });
            }
            catch
            {
                return View();
            }
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int id)
        {
            var q = db.Questions
                    .Include(r => r.Owner)
                    .Include(r => r.Set)
                    .SingleOrDefault(r => r.ID == id);


            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (q == null || !q.Owner.Equals(user)) return View("Error");

            return View(q);
        }

        // POST: Questions/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var q = db.Questions
                    .Include(r => r.Owner)
                    .Include(r => r.Set)
                    .SingleOrDefault(r => r.ID == id);
            

            //sprawdzanie czy jest wlascicielem
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (q == null || !q.Owner.Equals(user)) return View("Error");
            
            var setID = q.Set.ID;
            
            try
            {
                db.Questions.Remove(q);
                db.SaveChanges();
            }
            catch
            {
                Debug.WriteLine("Nie udalo sie usunac!");
            }

            return RedirectToAction("Manage", new { id = setID });
        }
    }
}
