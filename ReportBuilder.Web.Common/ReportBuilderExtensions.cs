using System;

namespace ReportBuilderAjax.Web.Common
{
    //TODO: this class is an exact copy of one in the silverlight project.  We need to see how the same library can
    //  be shared across both the SL client and the web proj
    public static class ReportBuilderExtensions
    {
        public static string CapitalizeEachWord(this String input)
        {
            string output = "";
            string[] values = input.ToLower().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string value in values)
            {
                output += uppercaseFirst(value) + " ";
            }
            return output.Trim();
        }

        public static string TrimDelimitedItems(this String input, string delimiter)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string output = string.Empty;
            string[] itemList = input.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string item in itemList)
            {
                if (string.IsNullOrEmpty(output))
                {
                    output += item.Trim();
                }
                else
                {
                    output += "," + item.Trim();
                }
            }
            return output;
        }

        #region Helper Methods
        private static string uppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
        #endregion

    }
}
