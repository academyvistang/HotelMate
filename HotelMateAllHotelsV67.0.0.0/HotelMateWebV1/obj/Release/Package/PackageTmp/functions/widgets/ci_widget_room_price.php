<?php
if( !class_exists('CI_Room_Price') ):

class CI_Room_Price extends WP_Widget {

	function CI_Room_Price(){
		$widget_ops = array('description' => __("Displays the room's price in the room's sidebar", 'ci_theme'));
		$control_ops = array(/*'width' => 300, 'height' => 400*/);
		parent::WP_Widget('ci_room_price_widget', $name='-= CI Room Price =-', $widget_ops, $control_ops);
	}

	function widget($args, $instance) {
		$ci_line1 = $instance['ci_line1'];
		$ci_line2 = $instance['ci_line2'];
		global $post;

		$ci_price = get_post_meta($post->ID, 'ci_cpt_room_from', true);

		extract($args);

		echo $before_widget;
		?>
		<div class="widget offer-widget room-price bs">
			<div>
				<span class="line1"><?php echo $ci_line1; ?></span>
				<span class="line2"><?php echo $ci_price; ?></span>
				<span class="line3"><?php echo $ci_line2; ?></span>
			</div>
		</div>
		<?php
		echo $after_widget;

	} // widget

	function update($new_instance, $old_instance){
		$instance = $old_instance;
		$instance['ci_line1'] = strip_tags($new_instance['ci_line1']);
		$instance['ci_line2'] = strip_tags($new_instance['ci_line2']);

		return $instance;
	} // save

	function form($instance) {
		$instance = wp_parse_args( (array) $instance, array( 'ci_line1' => '', 'ci_line2' => '' ));
		$ci_line1 = strip_tags($instance['ci_line1']);
		$ci_line2 = strip_tags($instance['ci_line2']);
		?>
		<p><?php _e('Enter the text you wish to be displayed before and after the price (e.g. "From:", "Per Night").', 'ci_theme'); ?></p>
		<p><label><?php _e('Text before price:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('ci_line1'); ?>" name="<?php echo $this->get_field_name('ci_line1'); ?>" type="text" value="<?php echo esc_attr($ci_line1); ?>" class="widefat" /></p>
		<p><label><?php _e('Text after price:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('ci_line2'); ?>" name="<?php echo $this->get_field_name('ci_line2'); ?>" type="text" value="<?php echo esc_attr($ci_line2); ?>" class="widefat" /></p>
		<?php
	} // form

} // class

register_widget('CI_Room_Price');

endif; // class_exists
?>
