using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Freestyle.Helpers.TypeConversion;

namespace HotelMateWebV1.Helpers
{
    public enum ActionType
    {
        /// <summary>
        /// No defined action
        /// </summary>
        None = 0,
        /// <summary>
        /// Add the item(s) to the collection
        /// </summary>
        Add = 1
        ,
        /// <summary>
        /// Remove the item(s) from the collection
        /// </summary>
        Remove = 2
    }

    public static class TerminalExtensions
    {
        /// <summary>
        /// Amends the course list cookie.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="action">The action.</param>
        /// <param name="courseCodes">The course codes.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <returns></returns>
        public static IEnumerable<string> AmendCourseListCookie(this HttpContextBase context, ActionType action, IEnumerable<string> courseCodes, string cookieName)
        {

            var original = Enumerable.Empty<string>();

            if (context.Request.Cookies[cookieName] != null)
            {
                original = context.Request.Cookies[cookieName].Value.ToList(',');
            }
            if (action == ActionType.None) return original;
            var courses = original.Except(courseCodes, StringComparer.OrdinalIgnoreCase);

            if (action == ActionType.Add)
            {
                courses = courseCodes.Concat(courses);
            }

            context.Response.Cookies[cookieName].Value = courses.ToDelimitedString(",");
            return courses;
        }

        public static IEnumerable<string> GetCourseListCookie(this HttpContextBase context, string cookieName)
        {
            var original = Enumerable.Empty<string>();

            if (context.Request.Cookies[cookieName] != null)
            {
                original = context.Request.Cookies[cookieName].Value.ToList(',');
            }

            return original;
        }

        public static void SetCourseListToNullCookie(this HttpContextBase context, string cookieName)
        {
            context.Response.Cookies[cookieName].Value = string.Empty;
        }

    }
}