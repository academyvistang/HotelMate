<?php
//
// Uncomment one of the following two. Their functions are in panel/generic.php
//
add_action('wp_enqueue_scripts', 'ci_enqueue_modernizr');
//add_action('wp_enqueue_scripts', 'ci_print_html5shim');


// This function lives in panel/generic.php
add_action('wp_footer', 'ci_print_selectivizr', 100);



add_action('init', 'ci_register_theme_scripts');
if( !function_exists('ci_register_theme_scripts') ):
function ci_register_theme_scripts()
{
	//
	// Register all scripts here, both front-end and admin. 
	// There is no need to register them conditionally, as the enqueueing can be conditional.
	//

	$key = '';
	if(ci_setting('google_maps_api_key')) {
		$key='key=' . ci_setting('google_maps_api_key') . '&';
	}
	$google_url = "http://maps.googleapis.com/maps/api/js?" . $key . "sensor=false";
	wp_register_script('ci-google-maps-api-3', $google_url, array(), null, false);

	wp_register_script('jq-dropkick', get_child_or_parent_file_uri('/js/dropkick.js'), array('jquery'), false, true);
	wp_register_script('jq-prettyPhoto', get_child_or_parent_file_uri('/js/jquery.prettyPhoto.js'), array('jquery'), false, true);
	wp_register_script('jq-equalHeights', get_child_or_parent_file_uri('/js/jquery.equalHeights.js'), array('jquery'), false, true);
	wp_register_script('jRespond', get_child_or_parent_file_uri('/js/jRespond.min.js'), array('jquery'), false, true);
	wp_register_script('jPanelMenu', get_child_or_parent_file_uri('/js/jquery.jpanelmenu.min.js'), array('jquery'), false, true);

	wp_register_script('ci-front-scripts', get_child_or_parent_file_uri('/js/scripts.js'),
		array(
			'jquery',
			'jquery-superfish',
			'jquery-ui-core',
			'jquery-ui-datepicker',
			'jquery-flexslider',
			'jq-prettyPhoto',
			'jq-dropkick',
			'jq-equalHeights',
			'jRespond',
			'jPanelMenu',
			'ci-google-maps-api-3'
		),
		CI_THEME_VERSION, true);
}
endif;


add_action('wp_enqueue_scripts', 'ci_enqueue_theme_scripts');
if( !function_exists('ci_enqueue_theme_scripts') ):
function ci_enqueue_theme_scripts()
{
	//
	// Enqueue all (or most) front-end scripts here.
	// They can be also enqueued from within template files.
	//	
	if ( is_singular() && get_option( 'thread_comments' ) ){
		wp_enqueue_script( 'comment-reply' );
	}

	wp_enqueue_script('ci-front-scripts');

	//
	// Map options export for ci-front-scripts
	//
	$coords = explode(',', ci_setting('map_coords'));
	$params['map_zoom_level'] = ci_setting('map_zoom_level');
	$params['map_coords_lat'] = $coords[0];
	$params['map_coords_long'] = $coords[1];
	$params['map_tooltip'] = ci_setting('map_tooltip');

	$params['slider_autoslide'] = ci_setting('slider_autoslide')=='enabled' ? true : false;
	$params['slider_effect'] = ci_setting('slider_effect');
	$params['slider_direction'] = ci_setting('slider_direction');
	$params['slider_duration'] = (string)ci_setting('slider_duration');
	$params['slider_speed'] = (string)ci_setting('slider_speed');

	wp_localize_script('ci-front-scripts', 'ThemeOption', $params);

}
endif;


if( !function_exists('ci_enqueue_admin_theme_scripts') ):
add_action('admin_enqueue_scripts','ci_enqueue_admin_theme_scripts');
function ci_enqueue_admin_theme_scripts() 
{
	global $pagenow;

	//
	// Enqueue here scripts that are to be loaded on all admin pages.
	//

	if(is_admin() and $pagenow=='themes.php' and isset($_GET['page']) and $_GET['page']=='ci_panel.php')
	{
		//
		// Enqueue here scripts that are to be loaded only on discounthotelmotels Settings panel.
		//

	}
}
endif;

?>
