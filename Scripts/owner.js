$(function () {

    //utworz klienta huba
    var chat = $.connection.quizHub;
    var currentQuestion;

    //rozpoczecie polaczenia i jego następstwa
    $.connection.hub.start((function () {

        //dolacz do quizu
        chat.server.joinQuiz($('#roomID').val());


        //startowanie quizu z przycisku ()
        $('#startButton').click(function ownerStart() {
            //wywolaj na hubie StartQuiz
            chat.server.startQuiz($('#roomID').val());
            $('#startButton').hide(200);

            //zeruj punkty
            var rows = $('#users tr');
            var i;
            for (i = 1; i < rows.length; i++) {
                var id = rows[i].id;
                var name = id.substr(0, id.length - 4);
                $('#' + name + '_points').text(0);
            }
        });
    }));






    //Deklaracja zdalnych funkcji klienckich dla wlasciciela:

    //uzytkownik dolaczyl
    chat.client.userJoined = function (name) {
        console.log(name + " dołączył");
        $('#users tbody').append('<tr id="' + name + '_row" class="border-2 border-light"><td>' + name + '</td><td id="' + name + '_points">0</td></tr>');
    }

    //uzytkownik wyszedl
    chat.client.userLeft = function (name) {
        rmUser(name);
    }

    //uzytkownik zdobyl punkt
    chat.client.addPoint = function (userName) {
        var prev = parseInt($('#' + userName + '_points').text());
        $('#' + userName + '_points').text(prev + 1);

        update(userName);
    }

    //quiz się zakończył, przesylanie wynikow
    chat.client.endQuiz = function () {
        //przeslij cala tabele
        chat.server.showRanking($('#rank').html(), $('#roomName').text());
        $('#startButton').show(200);
    }





    //Deklaracja lokalnych funkcji dla wlasciciela:


    //usuwanie uczestnika z listy
    function rmUser(name) {
        $('#users #' + name + '_row').remove();
    }

    function update(userName) {
        var thisRow = $('#' + userName + '_row');
        var mojepunkty = parseInt($('#' + userName + '_points').text());

        //znajdz miejsce w rankingu
        var rows = $('#users tr');
        var i;
        for (i = 1; i < rows.length; i++) {
            if (rows[i].id === thisRow.id) {
                break;
            }
        }

        //znajdz nowe miejsce
        for (var j = rows.length - 1; j > 0; j--) {
            var id = rows[j].id;
            var name = id.substr(0, id.length - 4);
            var jegopunkty = parseInt($('#' + name + '_points').text());
            if (mojepunkty > jegopunkty) {
                thisRow.insertBefore(rows[j]);
                if (j == 1) break;
            }
        }
    }
});