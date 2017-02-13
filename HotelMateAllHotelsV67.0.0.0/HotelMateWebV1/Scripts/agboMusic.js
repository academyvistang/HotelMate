
$(function () {

CloseMusicBox();

function CloseMusicBox() {
    $('.wrap_box1m').hide();
    $('.main_chat1m').addClass('hide_wrap_boxm');
}

function OpenMusicBox() {
    $('.wrap_box1m').show();
    $('.main_chat1m').removeClass('hide_wrap_boxm');
}


//Open
$('.open_chat1m').click(function () {
    $('.wrap_box1m').show();
    $('.main_chat1m').removeClass('hide_wrap_boxm');
});


$('.closed1m').click(function () {
    $('.wrap_box1m').hide();
    $('.main_chat1m').addClass('hide_wrap_boxm');
});



});