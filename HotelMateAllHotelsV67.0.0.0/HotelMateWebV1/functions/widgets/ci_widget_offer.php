<?php
if( !class_exists('CI_Offer') ):

class CI_Offer extends WP_Widget {

	function CI_Offer(){
		$widget_ops = array('description' => __('Display an offer.', 'ci_theme'));
		$control_ops = array(/*'width' => 300, 'height' => 400*/);
		parent::WP_Widget('ci_offer_widget', $name='-= CI Offer Widget =-', $widget_ops, $control_ops);
	}

	function widget($args, $instance) {
		$ci_line1 = $instance['ci_line1'];
		$ci_line2 = $instance['ci_line2'];
		$ci_line3 = $instance['ci_line3'];
		$offer_url = $instance['offer_url'];

		extract($args);

		?>
		<div class="widget offer-widget bs">
			<?php if ( !empty($offer_url) ) : ?>
				<a href="<?php echo $offer_url; ?>">
			<?php else: ?>
				<div>
			<?php endif; ?>

				<span class="line1"><?php echo $ci_line1; ?></span>
				<span class="line2"><?php echo $ci_line2; ?></span>
				<span class="line3"><?php echo $ci_line3; ?></span>

			<?php if ( !empty($offer_url) ) : ?>
				</a>
			<?php else: ?>
				</div>
			<?php endif; ?>
		</div>
		<?php

	} // widget

	function update($new_instance, $old_instance){
		$instance = $old_instance;
		$instance['ci_line1'] = strip_tags($new_instance['ci_line1']);
		$instance['ci_line2'] = strip_tags($new_instance['ci_line2']);
		$instance['ci_line3'] = strip_tags($new_instance['ci_line3']);
		$instance['offer_url'] = esc_url($new_instance['offer_url']);

		return $instance;
	} // save

	function form($instance) {
		$instance = wp_parse_args( (array) $instance, array( 'ci_line1' => '', 'ci_line2' => '', 'ci_line3' => '', 'offer_url' => '' ));
		$ci_line1 = strip_tags($instance['ci_line1']);
		$ci_line2 = strip_tags($instance['ci_line2']);
		$ci_line3 = strip_tags($instance['ci_line3']);
		$offer_url = esc_url($instance['offer_url']);
		?>
		<p><?php _e('Enter the text you wish to be displayed. Line 2 appears bigger than the rest.', 'ci_theme'); ?></p>
		<p><label><?php _e('Line 1:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('ci_line1'); ?>" name="<?php echo $this->get_field_name('ci_line1'); ?>" type="text" value="<?php echo esc_attr($ci_line1); ?>" class="widefat" /></p>
		<p><label><?php _e('Line 2:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('ci_line2'); ?>" name="<?php echo $this->get_field_name('ci_line2'); ?>" type="text" value="<?php echo esc_attr($ci_line2); ?>" class="widefat" /></p>
		<p><label><?php _e('Line 3:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('ci_line3'); ?>" name="<?php echo $this->get_field_name('ci_line3'); ?>" type="text" value="<?php echo esc_attr($ci_line3); ?>" class="widefat" /></p>
		<p><label><?php _e('URL (with http:// in front):', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('offer_url'); ?>" name="<?php echo $this->get_field_name('offer_url'); ?>" type="text" value="<?php echo esc_attr($offer_url); ?>" class="widefat" /></p>
		<?php
	} // form

} // class


register_widget('CI_Offer');

endif; // class_exists
?>
