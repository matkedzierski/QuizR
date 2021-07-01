using System.Data.Entity;
using Microsoft.AspNet.SignalR;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WATHoot2.Models;
using Microsoft.AspNet.Identity;
using System.Threading;

namespace WATHoot2.Classes
{
 
    public class QuizHub : Hub
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        // nawiązanie połączenia z hubem (bardziej do debugowania)
        public override Task OnConnected()
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //info na debug
            Debug.WriteLine(userName + " połączył się!");
            Debug.WriteLine("  ConnectionID: " + Context.ConnectionId);
            Debug.WriteLine("  UserID: " + userID);

            //metoda z klasy bazowej
            return base.OnConnected();
        }

        // dolaczanie uczestnika do quizu
        public void JoinQuiz(string name)
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //dodaj uzytkownika do grupy SignalR 
            Groups.Add(Context.ConnectionId, name);


            //znajdz w bazie pokoj i uzytkownika
            var room = db.Rooms.Include(r => r.Owner).FirstOrDefault(r => r.ID == name); //znajdz z bazy
            var user = db.Users.Find(userID);

            if (room.Owner.Id == userID) return; // nie dodawaj ownera do listy uczestnikow tylko do grupy signalR

            if (room.Users.Find(u => u.Id == user.Id) == null) // jesli jeszcze nie zawiera
            {
                room.Users.Add(user);
                db.SaveChanges();
            }

            //powiadom pozostalych o dolaczeniu
            Clients.Group(name).userJoined(userName);

            //info na debug
            Debug.WriteLine(userName + " dołączył do quizu: " + name);
            Debug.WriteLine("  ConnectionID: " + Context.ConnectionId);
            Debug.WriteLine("  UserID: " + userID);
        }

        // rozpoczęcie quizu
        public void StartQuiz(string name)
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //sprawdz czy jest ownerem
            var room = db.Rooms.Include(r => r.Owner).First(r => r.ID == name); //znajdz z bazy
            var owner = room.Owner;
            if (owner.Id != userID) return;

            //wczytaj z bazy zestaw pytan ownera
            var questionSet = db.QuestionSets.First(qs => qs.Owner.Id == userID);
            var n = questionSet.Questions.Count;

            //info na debug
            Debug.WriteLine(userName + " rozpoczął rozgrywke!");
            Debug.WriteLine("Liczba pytan: " + n);

            //wyslij do uczestnikow rozpoczecie quizu z liczba pytan i ConnID ownera ( w celu przekazywania odpowiedzi )
            Clients.OthersInGroup(name).startQuiz(n, Context.ConnectionId);

            //rozpocznij watek obslugi quizu, podajac nazwe quizu i liste pytan i conn ID ownera
            Thread quizThread = new Thread(HandleQuiz);
            var paramArr = new object[] { name, questionSet.Questions };
            quizThread.Start(paramArr);
        }

        // udzielenie odpowiedzi
        public void Answer(int questionID, string answer, string roomName, string ownerConnID)
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //sprawdz ownera
            //var room = db.Rooms.Include(r => r.Owner).First(r => r.ID == roomName); //znajdz z bazy
            //var owner = room.Owner;

            //wczytaj z bazy pytanie o takim ID i jego prawidlowa odpowiedz
            var question = db.Questions.First(q => q.ID == questionID);
            var correctAnswer = question.Correct;

            //sprawdz czy uczestnik odpowiedzial poprawnie
            var guessed = correctAnswer.ToUpper() == answer.ToUpper();

            //wyslij do uczestnikow rozpoczecie quizu
            Clients.Caller.reply(guessed);

            if (guessed)
                Clients.Client(ownerConnID).addPoint(userName);

            //info na debug
            Debug.WriteLine(userName + " odpowiedział " + (guessed ? "" : "nie") + "poprawnie na pytanie o ID: " + questionID);
            Debug.WriteLine(" Odpowiedź: " + answer + ", poprawna: " + correctAnswer);
        }

        //przeslij tabele z rankingiem od ownera do uczestnikow
        public void ShowRanking(string rankingJSON, string name)
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //sprawdz czy jest ownerem
            var room = db.Rooms.Include(r => r.Owner).First(r => r.ID == name); //znajdz z bazy
            var owner = room.Owner;
            if (owner.Id != userID) return;

            //info na debug
            Debug.WriteLine(userName + " zakończył rozgrywke!");

            Thread.Sleep(4000);
            //wyslij do uczestnikow zakonczenie quizu z liczba pytan i ConnID ownera ( w celu przekazywania odpowiedzi )
            Clients.OthersInGroup(name).showRanking(rankingJSON);
        }

        // opuszczanie quizu
        public void LeaveQuiz(string name)
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //usun uzytkownika z grupy SignalR
            Groups.Remove(Context.ConnectionId, name);

            //znajdz w bazie pokoj i usera
            var room = db.Rooms.Include(r => r.Owner).First(r => r.ID == name); //znajdz z bazy
            var roomUser = room?.Users.Find(u => u.Id == userID); // znajdz uzytkownika w kontekscie pokoju

            if (room.Owner.Id == userID) return; // nie usuwaj ownera z listy uczestnikow tylko z grupy signalR

            //usun z pokoju w bazie danych
            room?.Users.Remove(roomUser);
            db.SaveChanges();

            //powiadom pozostalych o opuszczeniu
            Clients.OthersInGroup(name).userLeft(userName);

            //info na debug
            Debug.WriteLine(userName + " opuścił quiz: " + name);
            Debug.WriteLine("  ConnectionID: " + Context.ConnectionId);
            Debug.WriteLine("  UserID: " + userID);
        }

        // rozłączenie z hubem (bardziej do debugowania)
        public override Task OnDisconnected(bool stopCalled)
        {
            //znajdz z kontekstu ID i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //info na debug
            Debug.WriteLine(userName + " rozłączył się!");
            Debug.WriteLine("  ConnectionID: " + Context.ConnectionId);
            Debug.WriteLine("  UserID: " + userID);

            //metoda z klasy bazowej
            return base.OnDisconnected(stopCalled);
        }



        // wysylaj kolejne pytanie w okreslonych interwalach (DZIAŁA NA WĄTKU)
        private void HandleQuiz(object paramArray)
        {
            //wczytaj parametry watku
            var pars = paramArray as object[];
            var name = pars[0] as string;
            var qs = pars[1] as List<Question>;
            var i = 1;
            var n = qs.Count;
            Thread.Sleep(5000);

            //wysylaj w okreslonych interwalach pytania i odpowiedzi
            foreach (Question q in qs)
            {
                Debug.WriteLine("Question: " + q.Content);
                Clients.OthersInGroup(name).nextQuestion(q.ID, i, n, q.Content, q.Aans, q.Bans, q.Cans, q.Dans);
                i++;
                Thread.Sleep(4000);
            }

            //powiadom wszystkich, wlacznie z ownerem o zakonczeniu
            Clients.Group(name).endQuiz();
        } 
    }
}