var selected = [" ", " ", " ", " ", " "];

$(document).ready(function () {
    $('#carddeck td').click(add);
    $('#arrow').click(calculate);
});

function add() {
    var index = selected.indexOf(" ");
    if (index == -1) {
        return;
    }
    selected.splice(index, 1, this.id);

    $('#selected' + index).css('background-image', 'url("resources/images/' + this.id + '@2x.png")');
    $('#selected' + index).click(remove);

    $(this).css('background-image', 'url("resources/images/Darken.png")');
    $(this).off('click').click(remove);
}

function remove() {
    var index, card, td, divSelected;
    if ($(this).is('td')) {
        card = this.id;
        index = selected.indexOf(card);
    } else {
        index = parseInt(this.id.substr(this.id.length - 1));
        card = selected[index];
    }
    td = $('#' + card);
    divSelected = $('#selected' + index);

    selected.splice(index, 1, " ");

    divSelected.css('background-image', 'url("resources/images/Darken@2x.png")');
    divSelected.off('click');

    td.css('background-image', 'url("resources/images/' + card + '.png")');
    td.off('click').click(add);

    $('#result_number').empty();
}

function calculate() {
    if (selected.indexOf(" ") != -1) {
        return;
    }

    var query = "";
    selected.forEach(function (card) {
        var items = card.split("_");
        switch (items[0]) {
            case "Hearts": query += "1,"; break;
            case "Spades": query += "2,"; break;
            case "Diamonds": query += "3,"; break;
            case "Clubs": query += "4,"; break;
        }

        switch (items[1]) {
            case "Ace": query += "1,"; break;
            case "King": query += "13,"; break;
            case "Queen": query += "12,"; break;
            case "Jack": query += "11,"; break;
            default: query += items[1] + ","; break;
        }
    });

    query = query.substring(0, query.length - 1);

    $('#result_number').empty();

    $.ajax({
        url: "http://localhost:45871/api/values/get",
        data: { hand: query },
        cache: false,
        success: function (hand) {
            $('#result_number').empty().append(hand.value);
        },
        error: function (request, status, error) {
            console.log(request.responseText);
        }
    });
}
