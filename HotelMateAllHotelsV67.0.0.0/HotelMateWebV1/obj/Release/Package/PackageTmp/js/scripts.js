
jQuery(window).on("load", function () {
   
	/* -----------------------------------------
	 Sliders Init
	 ----------------------------------------- */
	var $slider = jQuery("#home-slider");

	if ( $slider.length ) {
		$slider.flexslider({
			animation: ThemeOption.slider_effect,
			direction: ThemeOption.slider_direction,
			slideshow: Boolean(ThemeOption.slider_autoslide),
			slideshowSpeed: Number(ThemeOption.slider_speed),
			animationSpeed: Number(ThemeOption.slider_duration)
		});
	}


	jQuery(".slide-prev").on("click", function(e) {
		$slider.flexslider('prev');
		e.preventDefault();
	});

	jQuery(".slide-next").on("click", function(e) {
		$slider.flexslider('next');
		e.preventDefault();
	});

	jQuery('#room-carousel').flexslider({
		animation: "slide",
		animationLoop: true,
		slideshow: false,
		itemWidth: 180,
		asNavFor: '#room-gallery'
	});

	jQuery('#room-gallery').flexslider({
		controlNav: false,
		animationLoop: false,
		slideshow: false,
		sync: "#room-carousel"
	});

	/* -----------------------------------------
	 equalHeights init
	 ----------------------------------------- */
	jQuery('.room-list').equalHeights();
	jQuery('.gallery-list').equalHeights();

});

jQuery(document).ready(function($) {

	/* -----------------------------------------
	 Main Navigation Init
	 ----------------------------------------- */
	$('ul#navigation').superfish({
		delay:       300,
		animation:   {opacity:'show'},
		speed:       'fast',
		dropShadows: false
	});

	/* -----------------------------------------
	 Responsive Menus Init with jPanelMenu
	 ----------------------------------------- */
	var jPM = $.jPanelMenu({
		menu: '#navigation',
		trigger: '.menu-trigger',
		excludedPanelContent: "style, script, #wpadminbar"
	});

	var jRes = jRespond([
		{
			label: 'mobile',
			enter: 0,
			exit: 767
		}
	]);

	jRes.addFunc({
		breakpoint: 'mobile',
		enter: function() {
			jPM.on();
		},
		exit: function() {
			jPM.off();
		}
	});

	/* -----------------------------------------
	 Custom Dropdowns (Dropkick.js)
	 ----------------------------------------- */
	$(".b-form select").dropkick({
		change: function(value, label) {
			$(this).change();
		}
	});

	/* -----------------------------------------
	 LightBox Init (prettyPhoto.js)
	 ----------------------------------------- */
	$("a[rel^='prettyPhoto']").prettyPhoto({
		social_tools: '',
		deeplinking: false
	});

	$("a[data-rel^='prettyPhoto']").prettyPhoto({
		social_tools: '',
		deeplinking: false
	});

	

	/* -----------------------------------------
	 Datepicker Init (jQuery UI)
	 ----------------------------------------- */
	// The datepickers must output the format yy/mm/dd
	// otherwise PHP's checkdate() fails.
	// Makes sure arrival date is not after departure date, and vice versa.
	$( ".datepicker[name='arrived']" ).datepicker({
	    dateFormat: 'yy/mm/dd',minDate: 0,
		onSelect: function (dateText, dateObj)
		{
		    var departedDate = $(".datepicker[name='departed']").datepicker("getDate");
			var minDate = new Date(dateObj.selectedYear, dateObj.selectedMonth, dateObj.selectedDay );
			minDate.setDate(minDate.getDate()+1);
			$(".datepicker[name='departed']").datepicker("option", "minDate", minDate);
			

			if (departedDate <= minDate)
			{
			    $(".datepicker[name='departed']").datepicker('setDate', minDate);
			}
			else
			{
			    $(".datepicker[name='departed']").datepicker('setDate', departedDate);
			}
		}
	});

	$( ".datepicker[name='departed']" ).datepicker({
	    dateFormat: 'yy/mm/dd', minDate: 1,
		onSelect: function (dateText, dateObj)
		{
		    var arrivedDate = $(".datepicker[name='arrived']").datepicker("getDate");
			//var maxDate = new Date(dateText);
			var maxDate = new Date(dateObj.selectedYear, dateObj.selectedMonth, dateObj.selectedDay );
			maxDate.setDate(maxDate.getDate()-1);
			$(".datepicker[name='arrived']").datepicker("option", "maxDate", maxDate);
			$(".datepicker[name='arrived']").datepicker('setDate', arrivedDate);
		}
	});


	$(".datepicker[name='arrive']").datepicker({
	    dateFormat: 'yy/mm/dd', minDate: 0,
	    onSelect: function (dateText, dateObj)
	    {
	        var departedDate = $(".datepicker[name='depart']").datepicker("getDate");

	        var minDate = new Date(dateObj.selectedYear, dateObj.selectedMonth, dateObj.selectedDay);
	        minDate.setDate(minDate.getDate() + 1);
	        $(".datepicker[name='depart']").datepicker("option", "minDate", minDate);

	        if (departedDate <= minDate) {
	            $(".datepicker[name='depart']").datepicker('setDate', minDate);
	        }
	        else {
	            $(".datepicker[name='depart']").datepicker('setDate', departedDate);
	        }
	    }
	});

	$(".datepicker[name='depart']").datepicker({
	    dateFormat: 'yy/mm/dd', minDate: 1,
	    onSelect: function (dateText, dateObj) {
	        //var maxDate = new Date(dateText);
	        var maxDate = new Date(dateObj.selectedYear, dateObj.selectedMonth, dateObj.selectedDay);
	        maxDate.setDate(maxDate.getDate() - 1);
	        //alert("Here");
	        //$(".datepicker[name='arrive']").datepicker("option", "maxDate", maxDate);
	    }
	});



	/* -----------------------------------------
	 Datepicker Init (jQuery UI)
	 ----------------------------------------- */
	//if ( $('#map').length ) {
	//	map_init('map');
    //}
    //
	$('#ComplimentaryRoom').change(function () {
	    if ($(this).is(":checked")) {
	        var bs = $("#Room_BusinessPrice").val();
	        $("#GuestRoom.RoomRate").val(0.00);
	        $("#DiscountedRate").val(0.00);
	        $("#lbldiscountedRate").text("Complimentary Rate");
	        alert("You have chosen to make this room a complimentary room. The guest will not be charged for this room.");
	    } else {
	        var rr = $("#Room_Price").val();
	        $("#GuestRoom_RoomRate").val(rr);
	        $("#DiscountedRate").val(0.00);
	        $("#lbldiscountedRate").text("Discounted Rate");
	    }
	});

	$('#CompanyGuest').change(function () {
	   

	    if ($(this).is(":checked")) {
	        var bs = $("#Room_BusinessPrice").val();
	        $("#GuestRoom.RoomRate").val(bs);
	        $("#DiscountedRate").val(bs);
	        $("#lblRoomRate").text("Business Rate");	       
	        $("#ShowCompany").show();
	    } else
	    {
	        var rr = $("#Room_Price").val();
	        $("#GuestRoom_RoomRate").val(rr);
	        $("#DiscountedRate").val(0.00);
	        $("#lblRoomRate").text("Room Rate");
	        $("#ShowCompany").hide();
	    }
	});

	$('#InitialDeposit').blur(function () {	   
	    var valInitialDep = $('#InitialDeposit').val();
	    if(valInitialDep > 0)
	    {
	        $("#ShowPaymentMethod").show();
	    }
	    else
	    {
	        $("#ShowPaymentMethod").hide();
	    }
	});

    
	function GetHotelGuestByName(name) {

	    $('#gmail_loading').show();
	    $('#gmail_loading').hide();

	    $.ajax({
	        type: "get",
	        async: false,
	        url: "/Booking/GetGuestByName/",
	        data: { name: name },
	        dataType: "json",
	        success: function (data)
	        {
	            //alert("year");
	            var items = "";
	            $.each(data, function (i, item)
	            {
	                //<option class='level-0' value="1" selected="selected">None</option>
	                //<option class='level-0' value="1" selected='selected'>None</option>
	                //items += "<option class='level-0'  value=\"" + item.Value + "\">" + item.Text + "</option>";
	                items += "<option class='level-0' value='1' selected='selected'>None</option>";
	            });

	            
	            $("#GuestId").innerhtml("");
	            $("#GuestId").html("");
	            $("#GuestId").html(items);
	            alert("year35");
	            
	        }
	    }).done(function () {
	        $('#gmail_loading').hide();
	    });
	}

	function GetHotelGuest(telephone) {
	    
	    $('#gmail_loading').show();
	    $('#gmail_loading').hide();

	    $.ajax({
	        type: "get",
	        async: false,
	        url: "/Booking/GetGuest/",
	        data: { telephone: telephone },
	        dataType: "json",
	        success: function (data)
	        {
	            if (data.Found > 0)
	            {	                
	                $('#Guest_Telephone').val(data.Telephone);
	                $('#Guest_Address').val(data.Address);
	                $('#Guest_CarDetails').val(data.CarDetails);
	                $('#Guest_Email').val(data.Email);
	                $('#Guest_FullName').val(data.Fullname);
	                $('#Guest_Notes').val(data.Notes);
	            }
	        }
	    }).done(function ()
	    {
	        $('#gmail_loading').hide();
	    });
	}

	$('#RefreshBtn').click(function () {
	    
	    $('#Guest_Telephone').val('');
	    $('#Guest_Email').val('');
	    $('#Guest_CarDetails').val('');
	    $('#Guest_Address').val('');
	    $('#Guest_Notes').val('');
	    $('#Guest_FullName').val('');

	    return false;
	});

	$('#Guest_Telephone').blur(function ()
	{	    
	    var tel = $('#Guest_Telephone').val();
	    GetHotelGuest(tel);
	});

	//$('#Guest_FullName').blur(function () {
	//    var name = $('#Guest_FullName').val();
	//    GetHotelGuestByName(name);
	//});

	$("#target").submit(function (event) {
	    var vdiscountedRate = $("#DiscountedRate").val();

	    if (vdiscountedRate > 0) {
	        var yesno = confirm('You have choosen to overide the room rate and use the discounted/Business room rate, Do you want to continue?');

	        if (!yesno) {
	            event.preventDefault();
	        }
	    }
	});




	$('a.login-window').click(function () {

	    // Getting the variable's value from a link 
	    var loginBox = $(this).attr('href');

	    //Fade in the Popup and add close button
	    $(loginBox).fadeIn(300);

	    //Set the center alignment padding + border
	    var popMargTop = ($(loginBox).height() + 24) / 2;
	    var popMargLeft = ($(loginBox).width() + 24) / 2;

	    $(loginBox).css({
	        'margin-top': -popMargTop,
	        'margin-left': -popMargLeft
	    });

	    // Add the mask to body
	    $('body').append('<div id="mask"></div>');
	    $('#mask').fadeIn(300);

	    return false;
	});

    // When clicking on the button close or the mask layer the popup closed
	$('a.close, #mask').live('click', function () {
	    $('#mask , .login-popup').fadeOut(300, function () {
	        $('#mask').remove();
	    });
	    return false;
	});

});

function map_init(map_element) {
	myLatlng = new google.maps.LatLng(ThemeOption.map_coords_lat, ThemeOption.map_coords_long);

	var mapOptions = {
		zoom: parseInt(ThemeOption.map_zoom_level),
		center: myLatlng,
		mapTypeId: google.maps.MapTypeId.ROADMAP,
		scrollwheel: false
	};

	var map = new google.maps.Map(document.getElementById(map_element), mapOptions);

	var contentString = '<div class="content">'+ThemeOption.map_tooltip+'</div>';

	var infowindow = new google.maps.InfoWindow({
		content: contentString
	});

	var marker = new google.maps.Marker({
		position: myLatlng,
		map: map,
		title: ''
	});
	google.maps.event.addListener(marker, 'click', function() {
		infowindow.open(map,marker);
	});
}
