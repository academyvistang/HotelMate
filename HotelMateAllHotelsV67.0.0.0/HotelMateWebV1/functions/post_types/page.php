<?php
//
// Room listing template meta box
//

add_action('admin_init', 'ci_add_page_room_listing_meta');
add_action('save_post', 'ci_update_page_room_listing_meta');
if( !function_exists('ci_add_page_room_listing_meta') ):
function ci_add_page_room_listing_meta(){
	add_meta_box("ci_page_room_listing_meta", __('Room Listing Options', 'ci_theme'), "ci_add_page_room_listing_meta_box", "page", "normal", "high");
	add_meta_box("ci_page_gallery_listing_meta", __('Gallery Listing Options', 'ci_theme'), "ci_add_page_gallery_listing_meta_box", "page", "normal", "high");
}
endif;

if( !function_exists('ci_update_page_room_listing_meta') ):
function ci_update_page_room_listing_meta($post_id){
	if (defined('DOING_AUTOSAVE') && DOING_AUTOSAVE ) return;
	if (isset($_POST['post_view']) and $_POST['post_view']=='list') return;

	if (isset($_POST['post_type']) && $_POST['post_type'] == "page")
	{
		update_post_meta($post_id, "base_rooms_category", (isset($_POST["base_rooms_category"]) ? $_POST["base_rooms_category"] : '') );
		update_post_meta($post_id, "room_listing_columns", (isset($_POST["room_listing_columns"]) ? $_POST["room_listing_columns"] : 'four') );
		update_post_meta($post_id, "gallery_listing_columns", (isset($_POST["gallery_listing_columns"]) ? $_POST["gallery_listing_columns"] : 'four') );
		update_post_meta($post_id, "gallery_listing_sidebar", (isset($_POST["gallery_listing_sidebar"]) ? $_POST["gallery_listing_sidebar"] : 'four') );
		update_post_meta($post_id, "room_sidebar", (isset($_POST["room_sidebar"]) ? $_POST["room_sidebar"] : 'four') );
	}
}
endif;

if( !function_exists('ci_add_page_room_listing_meta_box') ):
function ci_add_page_room_listing_meta_box(){
	global $post;
	$category = get_post_meta($post->ID, 'base_rooms_category', true);
	$cols = get_post_meta($post->ID, 'room_listing_columns', true);
	$room_sidebar = get_post_meta($post->ID, 'room_sidebar', true);
	?>
	<p><?php _e('Select the base rooms category. Only rooms of the selected category and sub-categories will be displayed. If you don\'t select one (i.e. empty) all room categories will be shown. You need to select a <strong>Rooms Listing</strong> template for this option to work.', 'ci_theme'); ?></p>
	<?php wp_dropdown_categories(array(
		'selected'=>$category,
		'name' => 'base_rooms_category',
		'show_option_none' => ' ',
		'taxonomy' => 'room_category',
		'hierarchical' => 1,
		'show_count' => 1,
		'hide_empty' => 0
	)); ?>

	<p><?php _e('Select how many column should the grid of rooms be divided in.', 'ci_theme'); ?></p>
	<select name="room_listing_columns">
		<option value="six" <?php selected($cols, 'six'); ?>><?php _e('Two', 'ci_theme'); ?></option>
		<option value="four" <?php selected($cols, 'four'); ?>><?php _e('Three', 'ci_theme'); ?></option>
		<option value="three" <?php selected($cols, 'three'); ?>><?php _e('Four', 'ci_theme'); ?></option>
	</select>

	<p>
		<label for="room_sidebar">
			<input type="checkbox" id="room_sidebar" name="room_sidebar" <?php checked($room_sidebar, 'enabled'); ?> value='enabled'>
			<?php _e('Enable sidebar on room listing?', 'ci_theme'); ?>
		</label>
	</p>
	<?php
}
endif;

if( !function_exists('ci_add_page_gallery_listing_meta_box') ):
function ci_add_page_gallery_listing_meta_box(){
	global $post;
	$cols = get_post_meta($post->ID, 'gallery_listing_columns', true);
	$gallery_sidebar = get_post_meta($post->ID, 'gallery_listing_sidebar', true);
	?>
	<p><?php _e('Select how many column should the grid of galleries be divided in.', 'ci_theme'); ?></p>
	<select name="gallery_listing_columns">
		<option value="six" <?php selected($cols, 'six'); ?>><?php _e('Two', 'ci_theme'); ?></option>
		<option value="four" <?php selected($cols, 'four'); ?>><?php _e('Three', 'ci_theme'); ?></option>
		<option value="three" <?php selected($cols, 'three'); ?>><?php _e('Four', 'ci_theme'); ?></option>
	</select>

	<p>
	<label for="gallery_listing_sidebar">
		<input type="checkbox" id="gallery_listing_sidebar" name="gallery_listing_sidebar" <?php checked($gallery_sidebar, 'enabled'); ?> value="enabled" >
		<?php _e('Enable sidebar on gallery listing?', 'ci_theme'); ?>
	</label>
	</p>
	<?php
}
endif;

?>
