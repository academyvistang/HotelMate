<?php
add_action( 'widgets_init', 'ci_widgets_init' );
if( !function_exists('ci_widgets_init') ):
function ci_widgets_init() {

	register_sidebar(array(
		'name' => __( 'Blog Sidebar', 'ci_theme'),
		'id' => 'blog-sidebar',
		'description' => __( 'Sidebar on blog pages', 'ci_theme'),
		'before_widget' => '<aside id="%1$s" class="widget %2$s group">',
		'after_widget' => '</aside>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));
	
	register_sidebar(array(
		'name' => __( 'Rooms Sidebar', 'ci_theme'),
		'id' => 'room-sidebar',
		'description' => __( 'Sidebar for the room pages', 'ci_theme'),
		'before_widget' => '<aside id="%1$s" class="widget %2$s group">',
		'after_widget' => '</aside>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));
	
	register_sidebar(array(
		'name' => __( 'Pages Sidebar', 'ci_theme'),
		'id' => 'pages-sidebar',
		'description' => __( 'Sidebar for pages', 'ci_theme'),
		'before_widget' => '<aside id="%1$s" class="widget %2$s group">',
		'after_widget' => '</aside>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Header Social Widget', 'ci_theme'),
		'id' => 'header-sidebar',
		'description' => __( 'Header Widget, only the CI Socials Ignited widget is allowed here.', 'ci_theme'),
		'before_widget' => '<aside id="%1$s" class="widget %2$s group">',
		'after_widget' => '</aside>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Contact/Location page Sidebar', 'ci_theme'),
		'id' => 'contact-sidebar',
		'description' => __( 'Sidebar for the "Contact Page" template', 'ci_theme'),
		'before_widget' => '<aside id="%1$s" class="widget %2$s group">',
		'after_widget' => '</aside>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Homepage Sidebar 1', 'ci_theme'),
		'id' => 'next-to-slider',
		'description' => __( 'The widgets placed here will appear next to your slider in the widgetized homepage template.', 'ci_theme'),
		'before_widget' => '<aside id="%1$s" class="widget %2$s group">',
		'after_widget' => '</aside>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Homepage Sidebar 2 - Column 1', 'ci_theme'),
		'id' => 'home-mid-widgets-1',
		'description' => __( 'Homepage widget area at the bottom of the slider, first column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Homepage Sidebar 2 - Column 2', 'ci_theme'),
		'id' => 'home-mid-widgets-2',
		'description' => __( 'Homepage widget area at the bottom of the slider, second column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Homepage Sidebar 2 - Column 3', 'ci_theme'),
		'id' => 'home-mid-widgets-3',
		'description' => __( 'Homepage widget area at the bottom of the slider, third column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Homepage Sidebar 3', 'ci_theme'),
		'id' => 'home-sidebar-bottom',
		'description' => __( 'Homepage widget area next to the blog post.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Homepage Sidebar 4', 'ci_theme'),
		'id' => 'home-last',
		'description' => __( 'Full width widget area for the homepage, appears just before the footer.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'After Content Sidebar Column 1', 'ci_theme'),
		'id' => 'bottom-widgets-1',
		'description' => __( 'Widget area after the main content, all inner pages, first column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'After Content Sidebar Column 2', 'ci_theme'),
		'id' => 'bottom-widgets-2',
		'description' => __( 'Widget area after the main content, all inner pages, second column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'After Content Sidebar Column 3', 'ci_theme'),
		'id' => 'bottom-widgets-3',
		'description' => __( 'Widget area after the main content, all inner pages, third column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Footer Widget Column 1', 'ci_theme'),
		'id' => 'footer-widgets-1',
		'description' => __( 'Widget area on the footer, first column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Footer Widget Column 2', 'ci_theme'),
		'id' => 'footer-widgets-2',
		'description' => __( 'Widget area on the footer, second column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));

	register_sidebar(array(
		'name' => __( 'Footer Widget Column 3', 'ci_theme'),
		'id' => 'footer-widgets-3',
		'description' => __( 'Widget area on the footer, third column.', 'ci_theme'),
		'before_widget' => '<div id="%1$s" class="widget %2$s group">',
		'after_widget' => '</div>',
		'before_title' => '<h3 class="widget-title"><span>',
		'after_title' => '</span></h3>'
	));


}
endif;
?>
