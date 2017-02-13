jQuery(document).ready(function($) {

	//
	// Page edit scripts
	//
	if($('#page_template').length > 0) {
		var template_box = $('#page_template');

		var room_listing_metabox = $('div#ci_page_room_listing_meta');
		var gallery_listing_metabox = $('div#ci_page_gallery_listing_meta');

		// first run
		room_listing_metabox.hide();
		if( template_box.val() == 'template-rooms.php')
			room_listing_metabox.show();

		gallery_listing_metabox.hide();
		if( template_box.val() == 'template-galleries.php')
			gallery_listing_metabox.show();

		// show only the custom fields we need in the post screen
		$('#page_template').change(function(){
			if( template_box.val() == 'template-rooms.php')
					room_listing_metabox.show();
				else
					room_listing_metabox.hide();
					
			if( template_box.val() == 'template-galleries.php')
					gallery_listing_metabox.show();
				else
					gallery_listing_metabox.hide();
					
		});
		
	}


	//
	// Room edit scripts
	//
	
	// Repeating fields
	if($('#ci_cpt_room_meta').length > 0) {
		$('#ci_cpt_room_meta .amenities .inside').sortable();
		$('#amenities-add-field').click( function() {
			$('.amenities .inside').append('<p class="amenities-field"><input type="text" name="ci_cpt_room_amenities[]" /> <a href="#" class="amenities-remove">Remove me</a></p>');
			return false;		
		});
		$('#ci_cpt_room_meta').on('click', '.amenities-remove', function() {
			$(this).parent('p').remove();
			return false;
		});
	}

}); 
