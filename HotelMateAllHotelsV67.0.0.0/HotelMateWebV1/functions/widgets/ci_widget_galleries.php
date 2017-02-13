<?php
if( !class_exists('CI_Widget_Galleries') ):
	class CI_Widget_Galleries extends WP_Widget {

		function CI_Widget_Galleries(){
			$widget_ops = array('description' => __('Displays latest Photo Galleries', 'ci_theme'));
			$control_ops = array(/*'width' => 300, 'height' => 400*/);
			parent::WP_Widget('ci_galleries_widget', $name='-= CI Galleries =-', $widget_ops, $control_ops);
		}

		function widget($args, $instance) {
			global $post;

			extract($args);
			$title = apply_filters( 'widget_title', empty( $instance['title'] ) ? '' : $instance['title'], $instance, $this->id_base );
			$per_page = $instance['per_page'];
			$columns = $instance['columns'];

			$query_args = array(
				'post_type' => 'gallery',
				'posts_per_page' => $per_page
			);

			$q = new WP_Query($query_args);
			
			if ( $q->have_posts() ) :

				echo $before_widget;

				if ( $title )
					echo $before_title . $title . $after_title;

				echo '<div class="row">';
	
				while ( $q->have_posts() ) : $q->the_post();
					?>
					<div class="<?php echo $columns; ?> columns ci-gallery">
						<div class="g-wrap bs">
							<?php the_post_thumbnail('gallery_thumb'); ?>
							<div class="mask">
								<h3><a href="<?php the_permalink(); ?>"><?php the_title(); ?></a></h3>
								<p><?php echo mb_substr(get_the_excerpt(), 0, 200); ?></p>
								<a class="read-more btn" href="<?php the_permalink(); ?>"><?php _e('View Set', 'ci_theme'); ?></a>
							</div>
						</div>
					</div>
					<?php
				endwhile;

				echo '</div>';
				echo $after_widget;
				
			endif; // have_posts()
			wp_reset_postdata();
		}

		function update($new_instance, $old_instance) {

			$instance = $old_instance;
			$instance['per_page'] = intval($new_instance['per_page']);
			$instance['columns'] = strip_tags($new_instance['columns']);
			$instance['title'] = strip_tags($new_instance['title']);
			return $instance;

		}

		function form($instance) {
			$instance = wp_parse_args( (array) $instance, array('per_page' => 4, 'columns' => 'three', 'title' => '') );
			$per_page = intval($instance['per_page']);
			$columns = strip_tags($instance['columns']);
			$title = strip_tags($instance['title']);

			?>
			<p><?php _e("The widget will display a number of your latest galleries.", 'ci_theme'); ?></p>
			<p><label><?php _e('Title:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('title'); ?>" name="<?php echo $this->get_field_name('title'); ?>" type="text" value="<?php echo esc_attr($title); ?>" class="widefat" /></p>
			<p><label><?php _e('Number of Galleries:', 'ci_theme'); ?></label><input id="<?php echo $this->get_field_id('per_page'); ?>" name="<?php echo $this->get_field_name('per_page'); ?>" type="text" value="<?php echo esc_attr($per_page); ?>" class="widefat" /></p>

			<select name="<?php echo $this->get_field_name('columns'); ?>" class="widefat" id="<?php echo $this->get_field_id('columns'); ?>">
				<option value="twelve" <?php selected('twelve', $columns); ?>><?php _e('One Column', 'ci_theme'); ?></option>
				<option value="six" <?php selected('six', $columns); ?>><?php _e('Two Columns', 'ci_theme'); ?></option>
				<option value="four" <?php selected('four', $columns); ?>><?php _e('Three Columns', 'ci_theme'); ?></option>
				<option value="three" <?php selected('three', $columns); ?>><?php _e('Four Columns', 'ci_theme'); ?></option>
			</select>
			<?php
		}

	} // class

	register_widget('CI_Widget_Galleries');

endif; // class_exists
?>
