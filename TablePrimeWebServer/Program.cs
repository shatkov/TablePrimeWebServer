using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TablePrimeWebServer
{
    internal class Program
    {
        private static void Main()
        {
            // Необходима проверка на последний символ = слеш
            // Пример URL http://localhost:8080/prime/?min=1&max=15
            const string urlRes = "http://localhost:8080/prime/";
            var ws = new WebServer(SendResponse, urlRes);
            ws.Run();

            Console.WriteLine("A simple webserver. Press a key to quit.");

            Console.ReadKey();
            ws.Stop();
        }

        private static StringBuilder CreateHtmlDocument(StringBuilder str)
        {
            var sb = new StringBuilder();

            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("<meta charset=\"utf-8\">");
            sb.Append("<title>Таблица простых чисел</title>");
            sb.Append("<style>");
            sb.Append(@"table, th, td {
                            border: 1px solid black;
                        }");
            sb.Append("</style>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append(str);
            sb.Append("</body>");
            sb.Append("</html>");

            return sb;
        }

        private static StringBuilder CreateHtmlDocument(string str)
        {
            return CreateHtmlDocument(new StringBuilder(str));
        }

        private static string SendResponse(HttpListenerRequest request)
        {
            if (request.QueryString.Count <= 1 ||
                !long.TryParse(request.QueryString[0], out var min) ||
                !long.TryParse(request.QueryString[1], out var max))
            {
                return CreateHtmlDocument("Не верные параметры!").ToString();
            }

            var listRes = new List<long>();

            for (var i = min; i <= max; i++)
            {
                if (IsPrime(i))
                {
                    listRes.Add(i);
                }
            }

            for (var i = max; i < long.MaxValue; i++)
            {
                if (!IsPrime(i))
                {
                    continue;
                }

                listRes.Add(i);
                break;
            }

            var sb = new StringBuilder();

            sb.Append("<table>");
            sb.Append("<tr><th> Номер </th><th> Значение </th><th> Длина до следующего числа </th></tr>");

            for (var i = 0; i < listRes.Count - 1; i++)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + (i + 1) + "</td>");
                sb.Append("<td>" + listRes[i] + "</td>");
                sb.Append("<td>" + (listRes[i + 1] - listRes[i]) + "</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");

            return CreateHtmlDocument(sb).ToString();
        }

        private static bool IsPrime(long n)
        {
            var prime = n == 2 || (n != 1 && n % 2 != 0);

            for (long i = 3; prime && i * i <= n; i += 2)
            {
                prime = n % i != 0;
            }

            return prime;
        }
    }
}