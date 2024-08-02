using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFToolkit
{
    public partial class Toolkit
    {

        public static string ConvertSelectToDescriber(string SelectStatement)
        {
            string Prefix = "EXEC sp_describe_first_result_set @tsql = N' \n";

            string Body = SelectStatement.Replace("'", "") + "' \n";

            string Suffix = ", @params = NULL, @browse_information_mode = 0; \n \n" +
                "/*Once executed, copy the results table without headers into the\r\nTable Converter tool in Entity Framework Toolkit..*/";

            string DescribeCommand = Prefix + Body + Suffix;
            return DescribeCommand;
        }
    }
}
