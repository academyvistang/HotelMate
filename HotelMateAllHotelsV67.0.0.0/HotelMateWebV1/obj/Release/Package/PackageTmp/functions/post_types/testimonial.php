<?php
//
// Testimonials Post Type related functions.
//
add_action('init', 'ci_create_cpt_testimonials');
add_action('admin_init', 'ci_add_cpt_testimonials_meta');

if( !function_exists('ci_create_cpt_testimonials') ):
function ci_create_cpt_testimonials() {
	$labels = array(
		'name' => _x('Testimonials', 'post type general name', 'ci_theme'),
		'singular_name' => _x('Testimonial', 'post type singular name', 'ci_theme'),
		'add_new' => __('New Testimonial', 'ci_theme'),
		'add_new_item' => __('Add New Testimonial', 'ci_theme'),
		'edit_item' => __('Edit Testimonial', 'ci_theme'),
		'new_item' => __('New Testimonial', 'ci_theme'),
		'view_item' => __('View Testimonial', 'ci_theme'),
		'search_items' => __('Search Testimonials', 'ci_theme'),
		'not_found' =>  __('No Testimonials found', 'ci_theme'),
		'not_found_in_trash' => __('No Testimonials found in the trash', 'ci_theme'), 
		'parent_item_colon' => ''
	);

	$args = array(
		'labels' => $labels,
		'singular_label' => __('Testimonial', 'ci_theme'),
		'public' => true,
		'show_ui' => true,
		'capability_type' => 'post',
		'hierarchical' => false,
		'has_archive' => true,
		'rewrite' => array('slug' => 'testimonials'),
		'menu_position' => 5,
		'supports' => array('title', 'editor')
	);

	register_post_type( 'testimonial' , $args );
}
endif;

if( !function_exists('ci_add_cpt_testimonials_meta') ):
function ci_add_cpt_testimonials_meta(){
	add_meta_box("ci_cpt_testimonials_meta", __('Testimonial by...', 'ci_theme'), "ci_add_cpt_testimonials_meta_box", "testimonial", "normal", "high");
}
endif;

if( !function_exists('ci_add_cpt_testimonials_meta_box') ):
function ci_add_cpt_testimonials_meta_box(){
	?>	
	<label for="title"><?php _e('Use the main title, to denote who made the testimonial.', 'ci_theme'); ?></label>
	<?php

}
endif;

//
// Testimonials post type custom admin list
//
add_filter("manage_edit-testimonials_columns", "testimonials_edit_columns");  

if( !function_exists('testimonias_edit_columns') ):
function testimonias_edit_columns($columns){  
	$columns = array(  
		"cb" => "<input type=\"checkbox\" />",  
		"title" => __('Item Name', 'ci_theme')
	);  
	
	return $columns;  
}  
endif;
?>
