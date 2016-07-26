using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;

namespace EbayCrawlerWPF_2
{
    class Program
    {
        private static Mutex mutex = new Mutex();
        public static int THREADS = 4;
        public static int SUM_OF_PAGES = 0;
        public List<string> positive_list = new List<string>();
        public List<string> negative_list = new List<string>();
        private bool initialized = false;
        private Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
        public const string positive = "Positive";
        public const string negative = "Negative";
        private string _user;
        public string User
        {
            get
            {
                return _user;
            }
            set
            {
                _user = value;
            }
        }
        private string _product;
        public string Product
        {
            get
            {
                return _product;
            }
            set
            {
                _product = value;
            }
        }
        private void initialize()
        {
            if (dict != null && !initialized)
            {
                dict.Add(positive, positive_list);
                dict.Add(negative, negative_list);
                initialized = true;
            }
        }
        public string getSource(string link)
        {
            WebClient client = new WebClient();
            string htmlCode = client.DownloadString(link);
            return htmlCode;
        }
        private string getUserLink()
        {
            if (User.IndexOf("ebay.com") != -1)
            {
                return User;
            }
            else
            {
                return "http://www.ebay.com/usr/" + User;
            }
        }
        public bool checkUserID()
        {
            if (getSource(getUserLink()).IndexOf("The User ID you entered was not found") != -1)
            {
                return false;
            }
            return true;
        }
        /*
         * parameter could be positive, negative or neutral.
         * */
        private string getParameterLink(string htmlSource, string parameter)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(htmlSource);
            if (htmlDoc.DocumentNode != null) // gets the root of the document
            {
                HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
                if (bodyNode != null)
                {
                    string match = "//a[@title='" + parameter + "']";
                    string v = bodyNode.SelectSingleNode("//div[@id='feedback_ratings']").SelectSingleNode(match).WriteTo();
                    Regex reg = new Regex(@"<a href=\x22([^\x22]+)\x22");
                    return reg.Match(v).Groups[1].ToString();
                }
            }
            return "";
        }
        private List<string> searchPage(string htmlPage, string type)
        {
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(htmlPage);
            List<string> res = new List<string>();
            if (htmlDoc.DocumentNode != null) // gets the root of the document
            {
                HtmlAgilityPack.HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");
                if (bodyNode != null)
                {
                    HtmlAgilityPack.HtmlNode divNode = bodyNode.SelectSingleNode("//div[@class='CentralArea']");
                    HtmlAgilityPack.HtmlNode tableNode = divNode.SelectSingleNode("//table[@class='FbOuterYukon']");
                    int count = 0;
                    string feedback = String.Empty;
                    foreach (HtmlAgilityPack.HtmlNode node in tableNode.ChildNodes)
                    {
                        if (count == 0)
                        {
                            count++;
                            continue;
                        }
                        if (node.WriteContentTo().Contains("info90daysMsg"))
                        {
                            continue;
                        }
                        if (count % 2 == 1)
                        {
                            feedback = node.ChildNodes[1].InnerText;
                        }
                        else
                        {

                            //if (node.ChildNodes[1].InnerText.Contains(Product))
                            if (node.ChildNodes[1].InnerText.IndexOf(Product, StringComparison.CurrentCultureIgnoreCase) != -1)
                            {
                                res.Add(feedback);
                                if (type == positive)
                                {
                                    App.positiveWriter.Write(feedback);
                                }
                                else
                                {
                                    App.negativeWriter.Write(feedback);
                                }
                            }
                        }
                        count++;

                    }
                }
            }
            return res;
        }
        public void searchProductIn(object param)
        {
            string parameter = param.ToString();
            if (!CheckForInternetConnection())
            {
                throw new Exception("No Internet Connection");
            }
            if (!initialized)
            {
                initialize();
            }
            Console.WriteLine("Searching " + parameter);
            string link = getParameterLink(getSource(getUserLink()), parameter);
            string param_source = getSource(link);
            List<string> res = searchPage(param_source, parameter);
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(param_source);
            HtmlAgilityPack.HtmlNode pagination = htmlDoc.DocumentNode.SelectSingleNode("//body").SelectSingleNode("//div[@id='CentralArea']").SelectSingleNode("//div[@class='newPagination']");
            HtmlAgilityPack.HtmlNode pn_pagination = pagination.SelectSingleNode("//b[@id='PN_pagination1']");
            HtmlAgilityPack.HtmlNode pgn_pagination = pagination.SelectSingleNode("//b[@id='PGN_pagination1']");
            //Regex max_page_regex = new Regex(@">(\d+)</a></b>");
            //Console.WriteLine("Max Page: " + max_page_regex.Match(pgn_pagination.WriteTo()).Groups[1].ToString());
            //int max_pages = Int32.Parse(max_page_regex.Match(pgn_pagination.WriteTo()).Groups[1].ToString());
            /*
            if (mutex.WaitOne(1000))
            {
                SUM_OF_PAGES += max_pages;
                mutex.ReleaseMutex();
            }*/
            Regex reg = new Regex(@"<a href=\x22([^\x22]+)\x22");
            Regex page_num_regex = new Regex(@"page=(\d+)");
            while (pn_pagination.WriteTo().Contains("<a href="))
            {
                string next_page = reg.Match(pn_pagination.WriteTo()).Groups[1].ToString().Replace("amp;", "");
                string page_number = page_num_regex.Match(next_page).Groups[1].ToString();
                //Console.WriteLine("searching page: " + page_number);
                List<string> tmp = searchPage(getSource(next_page), parameter);
                /* if (mutex.WaitOne(1000))
                 {
                     Console.WriteLine("=====================");
                     mutex.ReleaseMutex();
                 }*/
                //Console.WriteLine("=====================");
                res = res.Concat(tmp).ToList();
                htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(getSource(next_page));
                pagination = htmlDoc.DocumentNode.SelectSingleNode("//body").SelectSingleNode("//div[@id='CentralArea']").SelectSingleNode("//div[@class='newPagination']");
                pn_pagination = pagination.SelectSingleNode("//b[@id='PN_pagination1']");
                if (mutex.WaitOne(1000))
                {
                    App.progressBar.Advance();
                    mutex.ReleaseMutex();
                }
            }
            dict[parameter] = dict[parameter].Concat(res).ToList();

        }
        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public int getSumPages()
        {
            string positive_link = getParameterLink(getSource(getUserLink()), positive);
            string negative_link = getParameterLink(getSource(getUserLink()), negative);
            string positive_source = getSource(positive_link);
            string negative_source = getSource(positive_link);
            //List<string> res = searchPage(param_source, parameter);
            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(positive_source);
            HtmlAgilityPack.HtmlNode pagination = htmlDoc.DocumentNode.SelectSingleNode("//body").SelectSingleNode("//div[@id='CentralArea']").SelectSingleNode("//div[@class='newPagination']");
            HtmlAgilityPack.HtmlNode pgn_pagination = pagination.SelectSingleNode("//b[@id='PGN_pagination1']");
            Regex max_page_regex = new Regex(@">(\d+)</a></b>");
            int max_pages = Int32.Parse(max_page_regex.Match(pgn_pagination.WriteTo()).Groups[1].ToString());
            SUM_OF_PAGES += max_pages;
            htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(negative_source);
            pagination = htmlDoc.DocumentNode.SelectSingleNode("//body").SelectSingleNode("//div[@id='CentralArea']").SelectSingleNode("//div[@class='newPagination']");
            pgn_pagination = pagination.SelectSingleNode("//b[@id='PGN_pagination1']");
            max_pages = Int32.Parse(max_page_regex.Match(pgn_pagination.WriteTo()).Groups[1].ToString());
            SUM_OF_PAGES += max_pages;
            return SUM_OF_PAGES;
        }
    }
    }
