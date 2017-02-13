using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace HotelMateWebV1.Extensions
{
    public static class CheckBoxExtensions
    {
    
        //    public static IList<string> ToList(this string value, params char[] delimiters)
    //    {
    //        if (delimiters == null || delimiters.Length == 0)
    //            throw new ArgumentException("You must provide as least one delimiter", "delimiters");
    //        List<string> list = new List<string>();
    //        if (!string.IsNullOrEmpty(value))
    //        {
    //            string[] strArray = Enumerable.ToArray<string>(Enumerable.Select<string, string>((IEnumerable<string>)value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries), (Func<string, string>)(s => s.Trim())));
    //            if (strArray != null)
    //                list.AddRange((IEnumerable<string>)strArray);
    //        }
    //        return (IList<string>)list.AsReadOnly();
    //    }

        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty[]>> expression, MultiSelectList multiSelectList, object htmlAttributes = null)
        {
            //Derive property name for checkbox name
            MemberExpression body = expression.Body as MemberExpression;
            string propertyName = body.Member.Name;

            //Get currently select values from the ViewData model
            TProperty[] list = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            //Convert selected value list to a List<string> for easy manipulation
            List<string> selectedValues = new List<string>();

            if (list != null)
            {
                selectedValues = new List<TProperty>(list).ConvertAll<string>(delegate(TProperty i) { return i.ToString(); });
            }

            //Create div
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            //Add checkboxes
            foreach (SelectListItem item in multiSelectList)
            {
                divTag.InnerHtml += String.Format("<div style='width:75px;float:left;'><input style='float:left;' type=\"checkbox\" name=\"{0}\" id=\"{0}_{1}\" " +
                                                    "value=\"{1}\" {2} /><label style='float:left; width:95px;padding-top:5px;padding-left:20px;' for=\"{0}_{1}\">{3}</label></div>",
                                                    propertyName.Trim(),
                                                    item.Value.Trim(),
                                                    selectedValues.Contains(item.Value) ? "checked=\"checked\"" : "",
                                                    item.Text.Trim());
            }

            return MvcHtmlString.Create(divTag.ToString());
        }
    }


}