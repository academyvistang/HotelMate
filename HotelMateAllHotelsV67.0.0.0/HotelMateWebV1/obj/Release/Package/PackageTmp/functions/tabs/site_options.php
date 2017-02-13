<?php global $ci, $ci_defaults, $load_defaults; ?>
<?php if ($load_defaults===TRUE): ?>
<?php
	add_filter('ci_panel_tabs', 'ci_add_tab_site_options', 10);
	if( !function_exists('ci_add_tab_site_options') ):
		function ci_add_tab_site_options($tabs) 
		{ 
			$tabs[sanitize_key(basename(__FILE__, '.php'))] = __('Site Options', 'ci_theme'); 
			return $tabs; 
		}
	endif;

	// Default values for options go here.
	// $ci_defaults['option_name'] = 'default_value';
	// or
	// load_panel_snippet( 'snippet_name' );

	$ci_defaults['layout'] = 'default';
	$ci_defaults['site_width']	= '1280';

	load_panel_snippet('logo');
	load_panel_snippet('favicon');
	load_panel_snippet('touch_favicon');
	load_panel_snippet('footer_text');

	add_action('wp_head', 'ci_site_width', 100);
	if ( !function_exists('ci_site_width') ):
		function ci_site_width() {
			?>
			<style type="text/css">
				.row { width: <?php echo intval(ci_setting('site_width')); ?>px; }
			</style>
		<?php
		}
	endif;
?>
<?php else: ?>
	
	<?php load_panel_snippet('logo'); ?>

	<fieldset class="set">
		<p class="guide"><?php _e('You can choose your site width. Recommended sizes: 1440, 1280 (default), 1140.' , 'ci_theme'); ?></p>
		<?php ci_panel_input('site_width', __('Enter your site width in pixels:', 'ci_theme')); ?>
	</fieldset>

	<fieldset class="set">
		<p class="guide"><?php _e('Select the layout of the site. This affects every post/page/etc of the site.', 'ci_theme'); ?></p>
		<?php 
			$layouts = array(
				'default' => _x('Default - Sidebar on the right', 'site layout', 'ci_theme'),
				'alt' => _x('Alternative - Sidebar on the left', 'site layout', 'ci_theme')
			);
			ci_panel_dropdown('layout', $layouts, __('Site layout', 'ci_theme'));
		?>
	</fieldset>

	<?php load_panel_snippet('favicon'); ?>

	<?php load_panel_snippet('touch_favicon'); ?>

	<?php load_panel_snippet('footer_text'); ?>

	<?php load_panel_snippet('sample_content'); ?>

<?php endif; ?>
