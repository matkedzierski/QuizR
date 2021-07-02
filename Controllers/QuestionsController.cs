using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using QuizR.Models;

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


        // GET: Questions
        [Authorize]
        public ActionResult Index()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var set = db.QuestionSets.FirstOrDefault(s => s.Owner.Id == user.Id);
            if(set == null)
            {
                set = new QuestionSet() { Owner = user };
                db.QuestionSets.Add(set);
                db.SaveChanges();
            }
            return View(set);
        }

        // GET: Questions/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Questions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Questions/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                string content = collection["Content"];
                string[] ans = new string[4];
                ans[0] = collection["Aans"];
                ans[1] = collection["Bans"];
                ans[2] = collection["Cans"];
                ans[3] = collection["Dans"];

                string correct = collection["Correct"];

                var user = UserManager.FindById(User.Identity.GetUserId());
                var set = db.QuestionSets.FirstOrDefault(s => s.Owner.Id == user.Id);
                var q = new Question() { Content = content, Aans = ans[0], Bans = ans[1], Cans = ans[2], Dans = ans[3], Correct = correct };
                set?.Questions.Add(q);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Questions/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int id)
        {
            var q = db.Questions.FirstOrDefault(r => r.ID == id);
            return View(q);
        }

        // POST: Questions/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                var q = db.Questions.FirstOrDefault(r => r.ID == id);
                db.Questions.Remove(q);
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
