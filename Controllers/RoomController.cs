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

        // GET: Room/Index
        public ActionResult Index(string RoomID)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            Debug.WriteLine("RoomID: " + RoomID);
            if (RoomID == null)
                return RedirectToAction("Index", "Home");
            else //jesli podano id pokoju
            {
                Debug.WriteLine("Finding: " + RoomID);
                var room = db.Rooms.Include(r => r.Owner).FirstOrDefault(r => r.ID == RoomID); //znajdz z bazy
                if (room == null) //jesli w bazie nie ma
                {
                    Debug.WriteLine("Creating: " + RoomID);
                    room = new QuizRoom() { ID = RoomID, Owner = user };
                    db.Rooms.Add(room);
                    //room.Users.Add(user);
                    db.SaveChanges();
                } else
                {
                    Debug.WriteLine("Found! Owner: " + room.Owner.Id);
                }
                return View(room);
            }
        }
    }

    public class RoomHub : Hub
    {
        public string RoomID { get; set; }
        public List<IdentityUser> Users { get; set; }

        public void Join(IdentityUser User)
        {
            Users.Add(User);
        }

    }
}