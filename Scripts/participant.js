$(function () {

    //utworz klienta huba
    var chat = $.connection.quizHub;
    var currentQuestion;

    //rozpoczecie polaczenia i jego następstwa
    $.connection.hub.start((function () {

        //dolacz do quizu
        chat.server.joinQuiz($('#roomID').val());


    }));





    //Deklaracja zdalnych funkcji klienckich dla uczestnika:


    //uzytkownik dolaczyl
    chat.client.userJoined = function (name) {
        chUserCount(1);
    }

    //uzytkownik wyszedl
    chat.client.userLeft = function (name) {
        chUserCount(-1);
    }

    //wlasciciel rozpoczal quiz (n - liczba pytan)
    chat.client.startQuiz = function (n, ownerConnID) {
        $('#before').hide(1000);
        $('#ranking').hide(1000);
        $('#qCount').text(n);
        $('#ownerConnID').text(ownerConnID);
        $('#starting').show(1000);
        $('#progressBar').css('width', 0 + '%').attr('aria-valuenow', 0);
    }


    //quiz przechodzi do nastepnego pytania
    chat.client.nextQuestion = function (qID, qNum, qTotal, content, a, b, c, d) {
        currentQuestion = qID;
        $('#before').hide(0);
        $('#reply').hide(200);
        $('#starting').hide(1000);
        $('#answers').hide(0);
        var progress = (qNum / qTotal) * 100;
        //schowaj, zmien wartosci i pokaz
        $('#inProgress').hide(800, function () {
            //numer pytania
            $('#qNum').text(qNum);
            $('#qTotal').text(qTotal);
            //tresc i odpowiedzi
            $('#qContent').text(content);
            $('#aAns').text(a);
            $('#bAns').text(b);
            $('#cAns').text(c);
            $('#dAns').text(d);

            //progressbar
            $('#answers').show();
            $('#inProgress').show(800);
            $('#progressBar').css('width', progress + '%').attr('aria-valuenow', progress);
        });
    }

    //quiz się zakończył, oczekiwanie na wyniki
    chat.client.endQuiz = function () {
        $('#inProgress').hide(1000);
        $('#ending').show(1000);
    }

    //wyswietlenie tabeli wynikow
    chat.client.showRanking = function (ranking) {
        console.log(ranking);
        $('#ending').hide(1000);
        $('#progressDiv').hide(1000);
        $('#rankingTab').html(ranking);
        $('#ranking').show(1000);
    }

    //Hub odpowiada czy poprawnie odpowiedzial czy nie
    chat.client.reply = function (correct) {
        if (correct) { // jesli poprawnie
            $('#reply').removeClass('btn-danger').addClass('btn-success').text('Dobrze!').show(200);
        } else {// jesli niepoprawnie
            $('#reply').removeClass('btn-success').addClass('btn-danger').text('Źle!').show(200);
        }

    }




    //Deklaracja lokalnych funkcji dla uczestnika:

    //przy zamykaniu strony opusc quiz
    window.addEventListener("beforeunload", function (e) {
        chat.server.leaveQuiz($('#roomID').val());
        return null;
    });

    //wyslanie odpowiedzi na pytanie do serwera
    $('#aAns, #bAns, #cAns, #dAns').click(function () {
        $('#answers').hide(0);
        var ans = this.id[0].toUpperCase();
        chat.server.answer(currentQuestion, ans, $('#roomID').val(), $('#ownerConnID').text());

    });


    //zmiana liczby uzytkownikow
    function chUserCount(x) {
        let nr = document.getElementById('userCount');
        let c = parseInt(nr.innerText);
        nr.innerText = (c + x);

        //let c = parseInt($('#userCount').text());
        //$('#userCount').text(c + x);
    }

});