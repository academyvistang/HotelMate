<?php 
if( !class_exists('CI_Room_Widget') ):

class CI_Room_Widget extends WP_Widget {

	function CI_Room_Widget(){
		$widget_ops = array('description' => __('Displays a room', 'ci_theme'));
		$control_ops = array(/*'width' => 300, 'height' => 400*/);
		parent::WP_Widget('ci_room_widget', $name='-= CI Room =-', $widget_ops, $control_ops);
	}
	
	function widget($args, $instance) {
		global $post;
		$old_post = $post;

		extract($args);
		$post_id = $instance['post_id'];

		$post = get_post($post_id);

		if($post !== null)
		{
			echo $before_widget;

			setup_postdata($post);
			?>
			<div class="bs entry-widget">
				<figure>
					<a href="<?php the_permalink(); ?>">
						<?php the_post_thumbnail('gallery_thumb'); ?>
					</a>
				</figure>

				<div class="content bg">
					<h3 class="title"><?php the_title(); ?></h3>
					<p><?php echo mb_substr(get_the_excerpt(), 0, 200); ?>&hellip;</p>
					<?php ci_read_more(); ?>
				</div>
			</div>
			<?php

			echo $after_widget;

		}

		$post = $old_post;
		setup_postdata($post);
	}

	function update($new_instance, $old_instance){
		$instance = $old_instance;
		$instance['post_id'] = intval($new_instance['post_id']);
		return $instance;
	}

	function form($instance){
		$instance = wp_parse_args( (array) $instance, array('post_id' => 0) );
		$post_id = intval($instance['post_id']);
		echo '<p><label for="'.$this->get_field_id('post_id').'">'.__('Select a Room to show:', 'ci_theme').'</label></p>';
		wp_dropdown_posts( array(
			'post_type' => 'room',
			'selected' => $post_id,
			'id' => $this->get_field_id('post_id')
		), $this->get_field_name('post_id'));
	}

} // class

register_widget('CI_Room_Widget');

endif; // class_exists
?>
