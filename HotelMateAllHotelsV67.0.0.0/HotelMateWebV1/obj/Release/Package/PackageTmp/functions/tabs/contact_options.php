<?php global $ci, $ci_defaults, $load_defaults; ?>
<?php if ($load_defaults===TRUE): ?>
<?php
	add_filter('ci_panel_tabs', 'ci_add_tab_contact_options', 50);
	if( !function_exists('ci_add_tab_contact_options') ):
		function ci_add_tab_contact_options($tabs) 
		{ 
			$tabs[sanitize_key(basename(__FILE__, '.php'))] = __('Contact Options', 'ci_theme'); 
			return $tabs; 
		}
	endif;
	
	// Default values for options go here.
	// $ci_defaults['option_name'] = 'default_value';
	// or
	// load_panel_snippet( 'snippet_name' );
	$ci_defaults['booking_form_page'] 	= '';
	$ci_defaults['booking_form_url'] 	= '';
	$ci_defaults['booking_form_email'] 	= get_option('admin_email');

	$ci_defaults['contact_show_map'] = 'enabled';

	$ci_defaults['map_tooltip'] = 'Pointblank Str. 14, 54321, California';
	$ci_defaults['map_coords'] = '33.59,-80';
	$ci_defaults['map_zoom_level'] = '6';

?>
<?php else: ?>

	<fieldset class="set">
		<p class="guide"><?php _e('Select your booking form page you have created and assigned the "Booking Form" page template to. This is to redirect properly when checking availability from any page on the site. If blank, the booking form will be hidden. The booking form e-mail address is where the e-mails will be sent.' , 'ci_theme'); ?></p>
		<fieldset>
			<label for="<?php echo THEME_OPTIONS; ?>[booking_form_page]"><?php _e('Select the Booking Form page', 'ci_theme'); ?></label>
			<?php wp_dropdown_pages(array(
				'show_option_none' => '&nbsp;',
				'selected'=>$ci['booking_form_page'],
				'name'=>THEME_OPTIONS.'[booking_form_page]'
			)); ?>
		</fieldset>

		<fieldset class="mt10">
			<?php ci_panel_input('booking_form_email', __('Booking form E-mail address', 'ci_theme')); ?>
		</fieldset>

		<fieldset>
			<p class="guide"><?php _e('If you hook your booking page to an external reservation system, ignore the booking page and place the URL of your system in the following input box (do not forget the http:// in front.)', 'ci_theme'); ?></p>
			<?php ci_panel_input('booking_form_url', __('External Booking URL', 'ci_theme')); ?>
		</fieldset>
	</fieldset>

	<fieldset class="set">
		<p class="guide"><?php _e('Map Settings: Here you can customize your map settings.', 'ci_theme');?> </p>
		<?php ci_panel_checkbox('contact_show_map', 'enabled', __('Enable the map', 'ci_theme')); ?>
		<?php ci_panel_input('map_coords', __('Enter the exact coordinates of your address (you can find your coordinates based on address using <a href="http://itouchmap.com/latlong.html">this tool</a>):', 'ci_theme')); ?>
		<?php ci_panel_input('map_zoom_level', __('Enter a single number from 1 to 20 that represents the default zoom level you want on your map. Higher number means closer.', 'ci_theme')); ?>
		<?php ci_panel_input('map_tooltip', __('Enter the text you wish to display when a user clicks on the map pin that points to your address (e.g. Your actual address):', 'ci_theme')); ?>
	</fieldset>
	
<?php endif; ?>
