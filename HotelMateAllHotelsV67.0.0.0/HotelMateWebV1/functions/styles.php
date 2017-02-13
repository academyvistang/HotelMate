<?php
add_action('init', 'ci_register_theme_styles');
if( !function_exists('ci_register_theme_styles') ):
function ci_register_theme_styles()
{
	//
	// Register all front-end and admin styles here. 
	// There is no need to register them conditionally, as the enqueueing can be conditional.
	//
	wp_register_style('foundation', get_child_or_parent_file_uri('/css/foundation.css'));
	wp_register_style('google-font', 'http://fonts.googleapis.com/css?family=Open+Sans:800');
	wp_register_style('jquery-ui', get_child_or_parent_file_uri('/css/redmond/jquery-ui-1.10.3.custom.min.css'));
	wp_register_style('flexslider', get_child_or_parent_file_uri('/css/flexslider.css'));
	wp_register_style('ci-mediaqueries', get_child_or_parent_file_uri('/css/mediaqueries.css'));
	wp_register_style('prettyPhoto', get_child_or_parent_file_uri('/css/prettyPhoto.css'));

	wp_register_style('ci-color-scheme', get_child_or_parent_file_uri('/colors/'.ci_setting('stylesheet')));

	wp_register_style('ci-style', get_stylesheet_uri(), 
		array(
			'google-font',
			'foundation',
			'jquery-ui',
			'flexslider',
			'prettyPhoto'
		), CI_THEME_VERSION, 'screen');

}
endif;

add_action('wp_enqueue_scripts', 'ci_enqueue_theme_styles');
if( !function_exists('ci_enqueue_theme_styles') ):
function ci_enqueue_theme_styles()
{
	//
	// Enqueue all (or most) front-end styles here.
	//	
	wp_enqueue_style('ci-style');
	wp_enqueue_style('ci-mediaqueries');
	wp_enqueue_style('ci-color-scheme');
}
endif;


if( !function_exists('ci_enqueue_admin_theme_styles') ):
add_action('admin_enqueue_scripts','ci_enqueue_admin_theme_styles');
function ci_enqueue_admin_theme_styles() 
{
	global $pagenow;

	//
	// Enqueue here styles that are to be loaded on all admin pages.
	//

	if(is_admin() and $pagenow=='themes.php' and isset($_GET['page']) and $_GET['page']=='ci_panel.php')
	{
		//
		// Enqueue here styles that are to be loaded only on discounthotelmotels Settings panel.
		//

	}
}
endif;

?>
