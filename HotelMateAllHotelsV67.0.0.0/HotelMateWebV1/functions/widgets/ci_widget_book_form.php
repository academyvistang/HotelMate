<?php
if( !class_exists('CI_Book_Form') ):

class CI_Book_Form extends WP_Widget {

	function CI_Book_Form(){
		$widget_ops = array('description' => __('Displays a Booking Form', 'ci_theme'));
		$control_ops = array('width' => 300, 'height' => 400);
		parent::WP_Widget('ci_booking_form_widget', $name='-= CI Booking Form =-', $widget_ops, $control_ops);
	}

	function widget($args, $instance) {
		extract($args);
		$title = apply_filters( 'widget_title', empty( $instance['title'] ) ? '' : $instance['title'], $instance, $this->id_base );
		$button = $instance['button'];
		$booking_note = $instance['booking_note'];

		echo ci_theme_show_booking_form_widget( array(
			'title' => $title,
			'note' => $booking_note,
			'button' => $button
		));

	} // widget

	function update($new_instance, $old_instance){
		$instance = $old_instance;
		$instance['title'] = strip_tags($new_instance['title']);
		$instance['button'] = strip_tags($new_instance['button']);
		$instance['booking_note'] = strip_tags($new_instance['booking_note']);
		return $instance;
	} // save

	function form($instance){
		$instance = wp_parse_args( (array) $instance, array('title'=>'', 'booking_note' => '', 'button' => __('Check Availability', 'ci_theme') ));
		$title = strip_tags($instance['title']);
		$button = strip_tags($instance['button']);
		$booking_note = strip_tags($instance['booking_note']);
		?>
		<p><?php _e("The widget will display a booking form which will redirect on either the theme's booking page or your custom URL supplied in the Theme Options.", 'ci_theme'); ?></p>
		<p><label><?php _e('Title:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('title'); ?>" name="<?php echo $this->get_field_name('title'); ?>" type="text" value="<?php echo esc_attr($title); ?>" class="widefat" /></p>
		<p><label><?php _e('Button text:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('button'); ?>" name="<?php echo $this->get_field_name('button'); ?>" type="text" value="<?php echo esc_attr($button); ?>" class="widefat" /></p>
		<p><label><?php _e('Booking Note:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('booking_note'); ?>" name="<?php echo $this->get_field_name('booking_note'); ?>" type="text" value="<?php echo esc_attr($booking_note); ?>" class="widefat" /></p>
		<?php
	} // form

} // class


register_widget('CI_Book_Form');

endif; // class_exists
?>
