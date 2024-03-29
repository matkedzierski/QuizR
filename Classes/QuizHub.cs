﻿using System.Data.Entity;
using Microsoft.AspNet.SignalR;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using QuizR.Models;
using Microsoft.AspNet.Identity;
using System.Threading;

namespace QuizR.Classes
{
 
    public class QuizHub : Hub
    {
        public ApplicationDbContext db = new ApplicationDbContext();

        // nawiązanie połączenia z hubem (bardziej do debugowania)
        public override Task OnConnected()
        {
            //znajdz z kontekstu Name i nick uzytkownika
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
        public void JoinQuiz(int id)
        {
            //znajdz z kontekstu Name i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //dodaj uzytkownika do grupy SignalR 
            Groups.Add(Context.ConnectionId, id.ToString());


            //znajdz w bazie pokoj i uzytkownika
            var room = db.Rooms.Include(r => r.Owner).FirstOrDefault(r => r.ID == id); //znajdz z bazy
            var user = db.Users.Find(userID);

            if (room.Owner.Id == userID) return; // nie dodawaj ownera do listy uczestnikow tylko do grupy signalR

            if (room.Users.Find(u => u.Id == user.Id) == null) // jesli jeszcze nie zawiera
            {
                room.Users.Add(user);
                db.SaveChanges();
            }

            //powiadom pozostalych o dolaczeniu
            Clients.Group(id.ToString()).userJoined(userName);

            //info na debug
            Debug.WriteLine(userName + " dołączył do quizu: " + id);
            Debug.WriteLine("  ConnectionID: " + Context.ConnectionId);
            Debug.WriteLine("  UserID: " + userID);
        }

        // rozpoczęcie quizu
        public void StartQuiz(int id)
        {
            //znajdz z kontekstu Name i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //sprawdz czy jest ownerem
            var room = db.Rooms
                .Include(r => r.Owner)
                .Include(r => r.Set)
                .First(r => r.ID == id); 
            var owner = room.Owner;
            if (owner.Id != userID) return;
            
            //wczytaj z bazy zestaw pytan powiazany z pokojem,
            var n = room.Set.Questions.Count;

            //info na debug
            Debug.WriteLine(userName + " rozpoczął rozgrywke!");
            Debug.WriteLine("Liczba pytan: " + n);

            //wyslij do uczestnikow rozpoczecie quizu z liczba pytan i ConnID ownera ( w celu przekazywania odpowiedzi )
            Clients.OthersInGroup(id.ToString()).startQuiz(n, Context.ConnectionId);

            //rozpocznij watek obslugi quizu, podajac nazwe quizu i liste pytan i conn Name ownera
            Thread quizThread = new Thread(HandleQuiz);
            var paramArr = new object[] { id, room.Set.Questions };
            quizThread.Start(paramArr);
        }

        // udzielenie odpowiedzi
        public void Answer(int questionID, string answer, string roomName, string ownerConnID)
        {
            //znajdz z kontekstu Name i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //sprawdz ownera
            //var room = db.Rooms.Include(r => r.Owner).First(r => r.Name == roomName); //znajdz z bazy
            //var owner = room.Owner;

            //wczytaj z bazy pytanie o takim Name i jego prawidlowa odpowiedz
            var question = db.Questions.First(q => q.ID == questionID);
            var correctAnswer = question.Correct;

            //sprawdz czy uczestnik odpowiedzial poprawnie
            var guessed = correctAnswer.ToUpper() == answer.ToUpper();

            //wyslij do uczestnikow rozpoczecie quizu
            Clients.Caller.reply(guessed);

            if (guessed)
                Clients.Client(ownerConnID).addPoint(userName);

            //info na debug
            Debug.WriteLine(userName + " odpowiedział " + (guessed ? "" : "nie") + "poprawnie na pytanie o Name: " + questionID);
            Debug.WriteLine(" Odpowiedź: " + answer + ", poprawna: " + correctAnswer);
        }

        //przeslij tabele z rankingiem od ownera do uczestnikow
        public void ShowRanking(string rankingJSON, int id)
        {
            //znajdz z kontekstu Name i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //sprawdz czy jest ownerem
            var room = db.Rooms.Include(r => r.Owner).First(r => r.ID == id); //znajdz z bazy
            var owner = room.Owner;
            if (owner.Id != userID) return;

            //info na debug
            Debug.WriteLine(userName + " zakończył rozgrywke!");

            Thread.Sleep(4000);
            //wyslij do uczestnikow zakonczenie quizu z liczba pytan i ConnID ownera ( w celu przekazywania odpowiedzi )
            Clients.OthersInGroup(id.ToString()).showRanking(rankingJSON);
        }

        // opuszczanie quizu
        public void LeaveQuiz(int id)
        {
            //znajdz z kontekstu Name i nick uzytkownika
            var userID = Context.User.Identity.GetUserId();
            var userName = Context.User.Identity.Name;

            //usun uzytkownika z grupy SignalR
            Groups.Remove(Context.ConnectionId, id.ToString());

            //znajdz w bazie pokoj i usera
            var room = db.Rooms.Include(r => r.Owner).First(r => r.ID == id); //znajdz z bazy
            var roomUser = room?.Users.Find(u => u.Id == userID); // znajdz uzytkownika w kontekscie pokoju

            if (room.Owner.Id == userID) return; // nie usuwaj ownera z listy uczestnikow tylko z grupy signalR

            //usun z pokoju w bazie danych
            room?.Users.Remove(roomUser);
            db.SaveChanges();

            //powiadom pozostalych o opuszczeniu
            Clients.OthersInGroup(id.ToString()).userLeft(userName);

            //info na debug
            Debug.WriteLine(userName + " opuścił quiz: " + id);
            Debug.WriteLine("  ConnectionID: " + Context.ConnectionId);
            Debug.WriteLine("  UserID: " + userID);
        }

        // rozłączenie z hubem (bardziej do debugowania)
        public override Task OnDisconnected(bool stopCalled)
        {
            //znajdz z kontekstu Name i nick uzytkownika
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
            var roomID = (int) pars[0];
            var qs = pars[1] as List<Question>;
            var i = 1;
            var n = qs.Count;
            Thread.Sleep(5000);

            //wysylaj w okreslonych interwalach pytania i odpowiedzi
            foreach (Question q in qs)
            {
                Debug.WriteLine("Question: " + q.Content);
                Clients.OthersInGroup(roomID.ToString()).nextQuestion(q.ID, i, n, q.Content, q.Aans, q.Bans, q.Cans, q.Dans);
                i++;
                Thread.Sleep(10000);
            }

            //powiadom wszystkich, wlacznie z ownerem o zakonczeniu
            Clients.Group(roomID.ToString()).endQuiz();
        } 
    }
}