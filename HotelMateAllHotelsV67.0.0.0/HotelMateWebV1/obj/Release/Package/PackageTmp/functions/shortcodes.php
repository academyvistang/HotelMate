<?php
/**
 * Theme Specific Shortcodes.
 *
 * @version		1.0.0
 * @package		functions/
 * @category	Shortcodes
 * @author 		discounthotelmotels
 */

/**
 * Used to output a shortcode without executing it.
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_shortcode') ) {
	function ci_shortcode($atts, $content = null ) {
		return '<code>' . $content .  '</code>';
	}

	add_shortcode('ci_shortcode', 'ci_shortcode');
} // function_exists('ci_shortcode')


/**
 * Outputs <div class="row"></div> container
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_row') ):
function ci_row($atts, $content = null ) {
	return '<div class="row">' . do_shortcode($content) . '</div>';
}
endif;
add_shortcode('ci_row', 'ci_row');
add_shortcode('ci_row2', 'ci_row');
add_shortcode('ci_row3', 'ci_row');


/**
 * Outputs columns markup
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_column') ):
function ci_column($atts, $content = null ) {
	extract(shortcode_atts(
		array(
			'span' => '',
			'mobile_span' => ''
		), $atts ));

	return '<div class="'. $span . ' ' . $mobile_span . ' columns">' . do_shortcode($content) . '</div>';
}
endif;
add_shortcode('ci_column', 'ci_column');


/**
 * Outputs fancy titles
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_big_title') ):
function ci_big_title( $atts, $content = null ) {
	return '<h3 class="ci-title"><span>' . $content . '</span></h3>';
}
endif;
add_shortcode('ci_big_title', 'ci_big_title');


/**
 * Outputs offer box row
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_offer') ):
function ci_offer( $atts, $content = null ) {

	extract(shortcode_atts( array(
		'url' => '',
		'text1' => '',
		'text2' => '',
		'text3' => ''
	), $atts ));

	return
		'<div class="widget offer-widget bs">'.
		'<a href="' . esc_url($url) . '">'.
		'<span class="line1">' . esc_html($text1) . '</span>'.
		'<span class="line2">' . esc_html($text2) . '</span>'.
		'<span class="line3">' . esc_html($text3) . '</span>'.
		'</a>'.
		'</div>';
}
endif;
add_shortcode('ci_offer', 'ci_offer');


/**
 * Outputs specified entry (user defined post type)
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_entry') ):
function ci_entry($atts, $content = null ) {

	extract(shortcode_atts(
		array(
			'type' => '',
			'id' => '',
			'slug' => ''
		), $atts ));

	$args = array (
		'post_type' => $type,
		'posts_per_page' => 1,
		'post_status' => 'publish',
		'ignore_sticky_posts' => true,
		'supress_filters' => false
	);

	if(empty($id) and empty($slug))
	{
		$args['p'] = $post->ID;
	}
	elseif(!empty($id) and $id > 0)
	{
		$args['p'] = $id;
	}
	elseif(!empty($slug))
	{
		$args['name'] = sanitize_title_with_dashes($slug, '', 'save');
	}
	global $post;

	$r = new WP_Query($args);

	if ( $r->have_posts() ) :
		while ( $r-> have_posts() ) : $r->the_post();
			$output =
				'<div class="widget bs entry-widget">'.
				'<figure>'.
				'<a href="' . get_permalink() . '">' . get_the_post_thumbnail($post->ID, "gallery_thumb") . '</a>'.
				'</figure>'.
				'<div class="content bg">'.
				'<h3 class="title">' . get_the_title() . '</h3>'.
				'<p>' . mb_substr(get_the_excerpt(), 0, 200) . '</p>'.
				'<a class="read-more btn" href="' . get_permalink() . '">' . ci_setting('read_more_text') . '</a>'.
				'</div></div>';
		endwhile;
	endif;
	wp_reset_postdata();
	return $output;
}
endif;
add_shortcode('ci_entry', 'ci_entry');


/**
 * Outputs specified room
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_room') ):
function ci_room($atts, $content = null ) {

	extract(shortcode_atts(
		array(
			'id' => '',
			'slug' => '',
			'limit' => '-1'
		), $atts ));

	$args = array (
		'post_type' => 'room',
		'posts_per_page' => 1,
		'post_status' => 'publish',
		'ignore_sticky_posts' => true,
		'supress_filters' => false
	);

	if(empty($id) and empty($slug))
	{
		$args['p'] = $post->ID;
	}
	elseif(!empty($id) and $id > 0)
	{
		$args['p'] = $id;
	}
	elseif(!empty($slug))
	{
		$args['name'] = sanitize_title_with_dashes($slug, '', 'save');
	}
	global $post;

	$r = new WP_Query($args);

	if ( $r->have_posts() ) :
		while ( $r-> have_posts() ) : $r->the_post();
			$output =
				'<div class="widget bs entry-widget">'.
				'<figure>'.
				'<a href="' . get_permalink() . '">' . get_the_post_thumbnail($post->ID, "gallery_thumb") . '</a>'.
				'</figure>'.
				'<div class="content bg">'.
				'<h3 class="title">' . get_the_title() . '</h3>'.
				'<p>' . mb_substr(get_the_excerpt(), 0, 200) . '</p>'.
				'<a class="read-more btn" href="' . get_permalink() . '">' . ci_setting('read_more_text') . '</a>'.
				'</div></div>';
		endwhile;
	endif;
	wp_reset_postdata();
	return $output;
}
endif;
add_shortcode('ci_room', 'ci_room');


/**
 * Outputs specified page
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_page') ):
function ci_page($atts, $content = null ) {

	extract(shortcode_atts(
		array(
			'id' => '',
			'slug' => '',
			'limit' => '-1'
		), $atts ));

	$args = array (
		'post_type' => 'page',
		'posts_per_page' => 1,
		'post_status' => 'publish',
		'ignore_sticky_posts' => true,
		'supress_filters' => false
	);

	if(empty($id) and empty($slug))
	{
		$args['p'] = $post->ID;
	}
	elseif(!empty($id) and $id > 0)
	{
		$args['p'] = $id;
	}
	elseif(!empty($slug))
	{
		$args['name'] = sanitize_title_with_dashes($slug, '', 'save');
	}
	global $post;

	$r = new WP_Query($args);

	if ( $r->have_posts() ) :
		while ( $r-> have_posts() ) : $r->the_post();
			$output =
				'<div class="widget bs entry-widget">'.
				'<figure>'.
				'<a href="' . get_permalink() . '">' . get_the_post_thumbnail($post->ID, "gallery_thumb") . '</a>'.
				'</figure>'.
				'<div class="content bg">'.
				'<h3 class="title">' . get_the_title() . '</h3>'.
				'<p>' . mb_substr(get_the_excerpt(), 0, 200) . '</p>'.
				'<a class="read-more btn" href="' . get_permalink() . '">' . ci_setting('read_more_text') . '</a>'.
				'</div></div>';
		endwhile;
	endif;
	wp_reset_postdata();
	return $output;
}
endif;
add_shortcode('ci_page', 'ci_page');


/**
 * Outputs a FlexSlider slider.
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_slider') ):
function ci_slider( $atts, $content = null ) {

	extract(shortcode_atts(
		array(), $atts ));

	global $post;

	$sld_q = new WP_Query( array(
		'post_type'=> 'slider',
		'posts_per_page' => -1
	));

	if ( $sld_q->have_posts() ) :
		$url = get_post_meta( $post->ID, 'ci_cpt_slider_url', true );
		$url = !empty($url) ? $url : get_permalink();

		$output =
			'<div class="slider-wrap">'.
			'<div id="home-slider" class="bs flexslider">'.
				'<ul class="slides">';
					while ( $sld_q->have_posts() ) : $sld_q->the_post();
				$output .=
						'<li>'.
						'<a href="' . $url . '">'.
						get_the_post_thumbnail($post->ID, 'slider_full').
						'<h3>'. get_the_title() . '</h3>'.
						'</a>'.
						'</li>';
					endwhile;
					wp_reset_postdata();
				$output .=
				'</ul>
		</div>'.
		'<div class="main-separator after-hero">
			<div class="home-slide-controls">
				<a class="slide-prev" href=""></a>
				<a class="slide-next" href=""></a>
			</div>
		</div>'.
		'</div>';
		return $output;
	else :
		return '';
	endif; // endif slider have_posts()
}
endif;
add_shortcode('ci_slider', 'ci_slider');


/**
 * Outputs a line separator
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_separator') ):
function ci_separator($atts, $content = null ) {
	return 	'<div class="row"><div class="twelve columns">' .
			'<div class="twelve columns main-separator"></div>' .
			'</div></div>';
}
endif;
add_shortcode('ci_separator', 'ci_separator');



if( !function_exists('ci_theme_show_booking_form_widget') ):
/**
 * Outputs a booking form widget
 *
 * @access public
 * @param array $atts
 * @return string
 */
function ci_theme_show_booking_form_widget($atts)
{
	extract(shortcode_atts( array(
		'title' => '',
		'note' => '',
		'button_text' => __('Check Availability', 'ci_theme')
	), $atts ));

	$book_page = ci_setting('booking_form_page');
	if ( !empty($book_page) )
		$form_action = get_permalink( $book_page );
	else
		$form_action = ci_setting('booking_form_url');


	ob_start();
	?>
	<div class="b-form widget bg bs">
		<form class="booking-form" action="<?php echo esc_url($form_action); ?>" method="post">
			<?php if( !empty($title) ): ?>
				<h3><?php echo $title; ?></h3>
			<?php endif; ?>
			
			<fieldset class="group">
				<p>
					<label for="arrive"><?php _e('Check In', 'ci_theme'); ?></label>
					<input readonly type="text" id="arrive" class="datepicker" name="arrive">
				</p>
	
				<p>
					<label for="depart"><?php _e('Check Out', 'ci_theme'); ?></label>
					<input readonly id="depart" class="datepicker" type="text" name="depart">
				</p>
			</fieldset>
	
			<fieldset class="group">
				<p>
					<label for="adults"><?php _e('Guests', 'ci_theme'); ?></label>
					<input type="number" min="1" max="6" id="adults" name="guests" value="1">
				</p>
	
				<p>
					<label for="b-room"><?php _e('Room', 'ci_theme'); ?></label>
					<?php
						$selected = is_singular('room') ? get_the_ID() : '';
		
						wp_dropdown_posts(array(
							'post_type' => 'room',
							'id' => 'b-room',
							'selected' => $selected
						), 'room_select');
					?>
				</p>
			</fieldset>
	
			<fieldset>
				<button class="btn b-book" type="submit"><?php echo $button_text; ?></button>
				<?php if( !empty($note) ): ?>
					<small class="booking-note"><?php echo $note; ?></small>
				<?php endif; ?>
			</fieldset>
		</form>
	</div>
	<?php
	$output = ob_get_clean();
	return $output;

}
endif;
add_shortcode('ci_book_form', 'ci_theme_show_booking_form_widget');


if( !function_exists('ci_theme_show_newsletter_widget') ):
/**
 * Outputs a booking form widget
 *
 * @access public
 * @param array $atts
 * @return string
 */
function ci_theme_show_newsletter_widget($atts)
{
	extract(shortcode_atts( array(
		'title' => '',
		'description' => '',
		'button_text' => __('Subscribe', 'ci_theme')
	), $atts ));


	ob_start();
	
	if(ci_setting('newsletter_action')!=''):
		?>
		<div class="widget ci-newsletter bg bs">
			<?php if( !empty($title) ): ?>
				<h3><?php echo $title; ?></h3>
			<?php endif; ?>
			<?php if( !empty($description) ): ?>
				<?php echo wpautop($description); ?>
			<?php endif; ?>

			<form class="newsletter-form" action="<?php ci_e_setting('newsletter_action'); ?>">
				<p>
					<input type="text" id="<?php ci_e_setting('newsletter_email_id'); ?>" name="<?php ci_e_setting('newsletter_email_name'); ?>" placeholder="<?php echo esc_attr(__('Your Email', 'ci_theme')); ?>">
					<input type="submit" class="btn" value="<?php echo esc_attr($button_text); ?>">
				</p>
				<?php
					$fields = ci_setting('newsletter_hidden_fields');
					if(is_array($fields) and count($fields) > 0)
					{
						for( $i = 0; $i < count($fields); $i+=2 )
						{
							if(empty($fields[$i]))
								continue;
							echo '<input type="hidden" name="'.esc_attr($fields[$i]).'" value="'.esc_attr($fields[$i+1]).'" />';
						}
					}
				?>
			</form>
		</div>
		<?php
	endif;
	
	$output = ob_get_clean();
	return $output;

}
endif;
add_shortcode('ci_newsletter', 'ci_theme_show_newsletter_widget');

/**
 * Outputs columns markup
 *
 * @access public
 * @param array $atts
 * @return string
 */

if ( !function_exists('ci_galleries') ):
	function ci_galleries($atts, $content = null ) {
		extract(shortcode_atts(
			array(
				'number' => '',
				'columns' => '3'
			), $atts ));

		if ( $columns == 2 ) {
			$cols = "six";
		} else if ( $columns == 3 ) {
			$cols = 'four';
		} else if ( $columns == 4 ) {
			$cols = 'three';
		} else if ( $columns == 1 ) {
			$cols = 'twelve';
		} else if ( $columns == 6 ) {
			$cols = 'two';
		}

		$query_args = array(
			'post_type' => 'gallery',
			'posts_per_page' => $number
		);

		$q = new WP_Query($query_args);

		ob_start();

		if ( $q->have_posts() ) :
			echo '<div class="row widget-ci-galleries">';
			while ( $q->have_posts() ) : $q->the_post();
		?>
			<div class="<?php echo $cols; ?> columns ci-gallery">
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
		endif; wp_reset_postdata();

		$output = ob_get_clean();
		return $output;
		}
endif;
add_shortcode('ci_galleries', 'ci_galleries');
