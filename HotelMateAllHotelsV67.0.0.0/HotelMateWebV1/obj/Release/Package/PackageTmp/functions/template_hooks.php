<?php 
add_filter('ci_footer_allowed_tags', 'ci_theme_footer_allowed_tags');
if( !function_exists('ci_theme_footer_allowed_tags') ):
function ci_theme_footer_allowed_tags($tags)
{
	$tags[] = '<br>';
	return $tags;
}
endif;

add_filter('ci_footer_credits', 'ci_theme_footer_credits');
if( !function_exists('ci_theme_footer_credits') ):
function ci_theme_footer_credits($string){

	if(!CI_WHITELABEL) {
		return '<a href="http://www.academyvista.com" '
			. 'title="'.esc_attr(__('Premium Leboston Themes', 'ci_theme')).'">' 
			. __('A theme by academyvista.com', 'ci_theme')
			. '</a>'
			.' &ndash; <span>'
			. __('Powered by Leboston', 'ci_theme') 
			. '</span>';
	}
	else {
		return '<a href="' . home_url() . '">' 
			. get_bloginfo('name') 
			. '</a> <br /> <span>' 
			. __('Powered by Leboston', 'ci_theme')
			. '</span>';
	} 

}
endif;
?>
