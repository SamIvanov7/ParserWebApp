using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Text;
using X.PagedList;

namespace ParserWebApp.Helpers
{
    public static class HtmlHelpers
    {
        public static IHtmlContent CustomPagedListPager(this IHtmlHelper html, IPagedList list, Func<int, string> generatePageUrl)
        {
            if (list.PageCount <= 1)
            {
                return HtmlString.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("<nav><ul class=\"pagination\">");

            // Prev page
            if (list.HasPreviousPage)
            {
                sb.Append($"<li class=\"page-item\"><a class=\"page-link\" href=\"{generatePageUrl(list.PageNumber - 1)}\">Previous</a></li>");
            }
            else
            {
                sb.Append("<li class=\"page-item disabled\"><span class=\"page-link\">Previous</span></li>");
            }

            // Page numbers
            int startPage = Math.Max(1, list.PageNumber - 5);
            int endPage = Math.Min(list.PageCount, startPage + 9);

            for (int i = startPage; i <= endPage; i++)
            {
                if (i == list.PageNumber)
                {
                    sb.Append($"<li class=\"page-item active\"><span class=\"page-link\">{i}</span></li>");
                }
                else
                {
                    sb.Append($"<li class=\"page-item\"><a class=\"page-link\" href=\"{generatePageUrl(i)}\">{i}</a></li>");
                }

                // separator
                if (i < endPage)
                {
                    sb.Append("<li class=\"page-item disabled\"><span class=\"page-link\">|</span></li>");
                }
            }

            // Next page
            if (list.HasNextPage)
            {
                sb.Append($"<li class=\"page-item\"><a class=\"page-link\" href=\"{generatePageUrl(list.PageNumber + 1)}\">Next</a></li>");
            }
            else
            {
                sb.Append("<li class=\"page-item disabled\"><span class=\"page-link\">Next</span></li>");
            }

            sb.Append("</ul></nav>");

            return new HtmlString(sb.ToString());
        }
    }
}
