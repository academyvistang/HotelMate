<?php
if( !class_exists('CI_Room_Amenities') ):
class CI_Room_Amenities extends WP_Widget {

	function CI_Room_Amenities(){
		$widget_ops = array('description' => __("Display Room Amenities in the respective Room's sidebar", 'ci_theme'));
		$control_ops = array(/*'width' => 300, 'height' => 400*/);
		parent::WP_Widget('ci_room_amenities_widget', $name='-= CI Room Amenities =-', $widget_ops, $control_ops);
	}

	function widget($args, $instance) {
		$title = apply_filters( 'widget_title', empty( $instance['title'] ) ? '' : $instance['title'], $instance, $this->id_base );
		$book_text = $instance['book_text'];

		global $post;

		$amenities = get_post_meta($post->ID, 'ci_cpt_room_amenities', true);
		extract($args);

		if ( count($amenities) > 0 and ( !empty($amenities) ) ) :
			?>
			<div class="ci-amenities bg bs">
				<?php
					if ( !empty( $title ) )
						echo '<h3>' . $title . '</h3>';
				?>
				<ul class="amenities">
					<?php
						foreach($amenities as $amenity)
							echo '<li>'. esc_html($amenity) .'</li>';
					?>
				</ul>
				<?php if( ci_setting('booking_form_page')!='' ): ?>
					<a class="btn b-book" href="<?php echo add_query_arg( array('room_select' => $post->ID), get_permalink(ci_setting('booking_form_page'))); ?>"><?php echo $book_text; ?></a>
				<?php elseif( ci_setting('booking_form_url') != '' ): ?>
					<a class="btn b-book" href="<?php echo add_query_arg( array('room_select' => $post->ID), ci_setting('booking_form_url')); ?>"><?php echo $book_text; ?></a>
				<?php endif; ?>
			</div>
			<?php
		endif; // if count amenities
	} // widget

	function update($new_instance, $old_instance){
		$instance = $old_instance;
		$instance['title'] = strip_tags($new_instance['title']);
		$instance['book_text'] = strip_tags($new_instance['book_text']);

		return $instance;
	} // save

	function form($instance){
		$instance = wp_parse_args( (array) $instance, array( 'book_text' => 'MAKE A NEW RESERVATION', 'title' => '' ));
		$title = strip_tags($instance['title']);
		$book_text = strip_tags($instance['book_text']);

		?>
		<p><?php _e('The widget will display the room\'s amenities in its sidebar, along with a book now button.', 'ci_theme'); ?></p>

	<?php
		echo '<p><label>' . __('Title:', 'ci_theme') . '</label><input id="' . $this->get_field_id('title') . '" name="' . $this->get_field_name('title') . '" type="text" value="' . esc_attr($title) . '" class="widefat" /></p>';

		echo '<p><label>' . __('Enter the text of your booking button:', 'ci_theme') . '</label><input id="' . $this->get_field_id('book_text') . '" name="' . $this->get_field_name('book_text') . '" type="text" value="' . esc_attr($book_text) . '" class="widefat" /></p>';

	} // form

} // class


register_widget('CI_Room_Amenities');

endif; // class_exists
?>
