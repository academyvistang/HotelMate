<?php 
if( !class_exists('CI_Page_Widget') ):
class CI_Page_Widget extends WP_Widget {

	function CI_Page_Widget(){
		$widget_ops = array('description' => __('Displays a single page with a featured image and excerpt', 'ci_theme'));
		$control_ops = array('width' => 300, 'height' => 400);
		parent::WP_Widget('ci_page_widget', $name='-= CI Page =-', $widget_ops, $control_ops);
	}
	
	function widget($args, $instance) {
		global $post;
		$old_post = $post;

		extract($args);

		$ci_post_id = $instance['post_id'];

		$post = get_post($ci_post_id);

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
		$ci_post_id = intval($instance['post_id']);
		echo '<p><label for="'.$this->get_field_id('post_id').'">'.__('Select a Page to show:', 'ci_theme').'</label></p>';
		wp_dropdown_pages( array(
			'selected' => $ci_post_id,
			'id' => $this->get_field_id('post_id'),
			'name' => $this->get_field_name('post_id')
		));
	}

} // class

register_widget('CI_Page_Widget');

endif; // class_exists
?>
