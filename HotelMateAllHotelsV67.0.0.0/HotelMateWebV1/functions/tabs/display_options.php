<?php global $ci, $ci_defaults, $load_defaults, $content_width; ?>
<?php if ($load_defaults===TRUE): ?>
<?php
	add_filter('ci_panel_tabs', 'ci_add_tab_display_options', 30);
	if( !function_exists('ci_add_tab_display_options') ):
		function ci_add_tab_display_options($tabs) 
		{ 
			$tabs[sanitize_key(basename(__FILE__, '.php'))] = __('Display Options', 'ci_theme'); 
			return $tabs; 
		}
	endif;
	
	// Default values for options go here.
	// $ci_defaults['option_name'] = 'default_value';
	// or
	// load_panel_snippet( 'snippet_name' );
	load_panel_snippet('excerpt');
	load_panel_snippet('seo');
	load_panel_snippet('comments');

	$ci_defaults['header_phone'] 	= '555-HOTEL-61';
	$ci_defaults['phone_url'] 		= 'http://www.academyvista.com/';
	$ci_defaults['header_email'] 	= 'info@hotelmotel.ci';
	$ci_defaults['read_more_text'] 	= 'Read More';


	add_filter('ci_featured_image_post_types', 'ci_add_featured_img_cpt');
	if( !function_exists('ci_add_featured_img_cpt') ):
	function ci_add_featured_img_cpt($post_types)
	{
		$post_types[] = 'slider';
		return $post_types;		
	}

	endif;
	load_panel_snippet('featured_image_single');

	// Change the ci_read_more() markup
	add_filter('ci-read-more-link', 'ci_theme_readmore', 10, 3);
	if( !function_exists('ci_theme_readmore') ):
	function ci_theme_readmore($html, $text, $link)
	{
		return '<a class="read-more btn" href="'.$link.'">'.$text.'</a>';
	}
	endif;

	// Set our full width template width and options.
	add_filter('ci_full_template_width', 'ci_fullwidth_width');
	if( !function_exists('ci_fullwidth_width') ):
	function ci_fullwidth_width()
	{ 
		return intval(ci_setting('site_width'));
	}
	endif;
	load_panel_snippet('featured_image_fullwidth');

?>
<?php else: ?>
		
	<fieldset class="set">
		<p class="guide"><?php _e('Enter your e-mail as it will appear on the top of your website.', 'ci_theme'); ?></p>
		<fieldset>
			<?php ci_panel_input('header_email', __('Header Email', 'ci_theme')); ?>
		</fieldset>

		<p class="guide"><?php _e('Enter your phone number as you want it appeared at the top of your website.', 'ci_theme'); ?></p>
		<fieldset>
			<?php ci_panel_input('header_phone', __('Header Phone Number:', 'ci_theme')); ?>
		</fieldset>

		<p class="guide"><?php _e('Enter a URL (presumably the contact page) where you want the phone number link to go to.', 'ci_theme'); ?></p>
		<fieldset>
			<?php ci_panel_input('phone_url', __('Header Phone Link URL:', 'ci_theme')); ?>
		</fieldset>
	</fieldset>
	
	<?php load_panel_snippet('excerpt'); ?>	

	<?php load_panel_snippet('seo'); ?>	

	<?php load_panel_snippet('comments'); ?>

	<?php load_panel_snippet('featured_image_single'); ?>

	<?php load_panel_snippet('featured_image_fullwidth'); ?>
		
<?php endif; ?>
