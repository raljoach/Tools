using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.PhantomJS;
using System.IO;
using System.Drawing;
using System.Collections.ObjectModel;
using OpenQA.Selenium.Remote;
using System.Threading;
using InterviewsLib;
using Newtonsoft.Json;

namespace CareerCupToolkit
{
    public enum MenuType
    {
        None,
        Sites,
        CareerCup,
        GlassDoor,
        Topic,
        Company,
        Keywords,
        Custom,
        Mode
    }
    public class OptionsResult
    {
        public Option[] options;
        public Dictionary<string, int> shortcutHash;
        public Dictionary<int, int> indexHash;
        internal string chosen;

        public OptionsResult(Option[] options, Dictionary<string, int> shortcutHash, Dictionary<int, int> indexHash)
        {
            this.options = options;
            this.shortcutHash = shortcutHash;
            this.indexHash = indexHash;
        }
    }
    public class Program
    {
        private static string careerCupSite = "https://careercup.com/";
        private static string website = careerCupSite;

        static void Main(string[] args)
        {
            //Prototype();
            ShowMenu(new Menu(MenuType.Sites, null));
            Console.WriteLine("Program has ended. Hit any key to exit.");
            Console.ReadKey();
        }

        private static Menu currentMenu = null;
        private static HashSet<string> keywords = new HashSet<string>();
        private static HashSet<string> standardKeyWords =
            new HashSet<string>() {
                "linear", "quadratic",
                "O(", "space", "cubic", "runtime", "time", "n^", "optim", "tree",
                "graph", "recursion", "sort", "search"};

        private static OptionsResult ShowMenu(Menu menu)
        {
            var prevMenu = menu.Previous;
            var temp = currentMenu;
            currentMenu = menu;
            switch (menu.MenuType)
            {
                case MenuType.Custom:
                    return (menu as CustomMenu).Show();
                case MenuType.Sites:
                    return ShowOptions(
                        "Select a site to search for questions:",
                        new Option(
                            "CareerCup",
                            "c",
                            () => { ShowMenu(new Menu(MenuType.CareerCup, menu)); }),
                        new Option(
                            "GlassDoor",
                            "g",
                            () => { ShowMenu(new Menu(MenuType.GlassDoor, menu)); }));
                //break;                
                case MenuType.CareerCup:
                    return ShowOptions(
                        "Select one of the following:",
                        new Option(
                            "Filter by company",
                            "c",
                            () => { ShowMenu(new Menu(MenuType.Company, menu)); }),

                        menu.MenuType == MenuType.CareerCup ? new Option(
                            "Filter by topic",
                            "t",
                            () => { ShowMenu(new Menu(MenuType.Topic, menu)); }) : null,
                        new Option(
                            "Filter by search terms",
                            "k",
                            () =>
                            {
                                ShowMenu(new Menu(MenuType.Keywords, menu));
                            }),
                        //new Option("Set Help","h"),
                        new Option(
                            "Begin Search",
                            "s",
                            () =>
                            {
                                IWebDriver driver = InitPhantomJS();
                                try
                                {
                                    DisplayQuestions(driver, selectedCompany, selectedTopic);
                                }
                                finally
                                {
                                    driver.Quit();
                                }
                            }),
                        new Option(
                            "Resume Search",
                            "r",
                            () =>
                            {
                                Console.WriteLine("Enter page number:");
                                int pageNum = int.Parse(Console.ReadLine());
                                Console.WriteLine("Enter question number:");
                                int qNum = int.Parse(Console.ReadLine());
                                IWebDriver driver = InitPhantomJS();
                                try
                                {
                                    DisplayQuestions(driver, selectedCompany, selectedTopic, pageNum, qNum);
                                }
                                finally
                                {
                                    driver.Quit();
                                }
                            }),
                            PreviousMenu(menu.Previous)
                        );
                //break;                
                case MenuType.Keywords:
                    List<Option> options = ListOptions(menu);
                    options.Add(PreviousMenu(prevMenu));
                    return ShowOptions(
                        "Select one of the following:",
                        options.ToArray()
                        );
                //break;
                case MenuType.Topic:
                    return ShowOptions(
                        "Select one of the following:",
                        new Option(
                            "Select topic",
                            "",
                            () =>
                            {
                                ShowCareerCupTopics();
                                ShowMenu(prevMenu);
                            }),
                        PreviousMenu(prevMenu)
                        );
                //break;
                case MenuType.Company:
                    return ShowOptions(
                        "Select one of the following:",
                        new Option(
                            "Select company",
                            "",
                            () =>
                            {
                                ShowCareerCupCompanies();
                                ShowMenu(prevMenu);
                            }),
                        PreviousMenu(prevMenu)
                        );
                //break;
                default:
                    currentMenu = temp;
                    throw new InvalidOperationException(
                        string.Format("Error: Unhandled menu type:{0}", menu));
            }
        }

        private static List<Option> ListOptions(Menu menu)
        {
            Menu prevMenu = menu.Previous;
            List<Option> options = null;
            switch (menu.MenuType)
            {
                case MenuType.Keywords:
                    options =
                        new List<Option>
                        {
                            new Option(
                            "Add keyword/search terms",
                            "a",
                            () => {
                                AddSearchTerms();
                                ShowMenu(prevMenu);
                            }),
                            new Option(
                            "Get keyword/search terms",
                            "g",
                            () => {
                                GetSearchTerms();
                                ShowMenu(prevMenu);
                            }),
                            new Option(
                            "Remove keyword/search terms",
                            "r",
                            () => {
                                RemoveSearchTerms();
                                ShowMenu(prevMenu);
                            }),
                        new Option(
                            "Clear all keyword/search terms",
                            "c",
                            () => {
                                ClearSearchTerms();
                                ShowMenu(prevMenu);
                            }),
                        new Option(
                            "Use Standard list of keyword/search terms",
                            "s",
                            () => {
                                AddSearchTerms(standardKeyWords);
                                ShowMenu(prevMenu);
                            })
                        };
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Error: Invalid menu chosen '{0}'", menu));
            }

            return options;
        }

        private static string selectedTopic;
        private static string selectedCompany;

        private static void ShowCareerCupTopics()
        {
            List<string> topics = GetCareerCupTopics();
            bool valid = false;
            int index = -1;
            while (!valid)
            {
                ShowOptions("Choose from one of the following topics", topics.ToArray());
                Console.WriteLine();
                Console.Write(":");
                index = int.Parse(Console.ReadLine()) - 1;
                if (index < 0 || index > topics.Count) { Console.WriteLine(string.Format("Error: Invalid choice '{0}'", index + 1)); continue; }
                valid = true;
            }
            var chosen = topics[index];
            selectedTopic = chosen;
        }

        private static void ShowCareerCupCompanies()
        {
            List<string> companies = GetCareerCupCompanies();
            bool valid = false;
            int index = -1;
            while (!valid)
            {
                ShowOptions("Choose from one of the following companies", companies.ToArray());
                Console.Write(":");
                index = int.Parse(Console.ReadLine()) - 1;
                Console.WriteLine();
                if (index < 0 || index > companies.Count) { Console.WriteLine(string.Format("Error: Invalid choice '{0}'", index + 1)); continue; }
                valid = true;
            }
            var chosen = companies[index];
            selectedCompany = chosen;
        }

        private static List<string> careerCupTopics = null;
        private static List<string> GetCareerCupTopics()
        {
            if (careerCupTopics == null)
            {
                careerCupTopics = new List<string>();

                Debug("Wait one moment, while I query for topics...");
                IWebDriver driver = InitPhantomJS();
                try
                {
                    Debug("Logging into CareerCup website...");
                    driver.Navigate().GoToUrl(website);

                    Debug("Searching for topics...");
                    IWebElement questionsLink = driver.FindElement(By.LinkText("Questions"));
                    questionsLink.Click();

                    var topicsCombo = driver.FindElement(By.Id("topic"));
                    var options = topicsCombo.FindElements(By.TagName("option"));

                    if (options == null || options.Count == 0)
                    {
                        throw new InvalidOperationException("Error: Unable to find list of topics!");
                    }

                    Debug("Successfully Retrieved topics");
                    Console.WriteLine();
                    foreach (var o in options)
                    {
                        careerCupTopics.Add(o.Text);
                    }
                }
                finally
                {
                    driver.Quit();
                }
            }
            return careerCupTopics;
        }

        private static void Debug(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        private static List<string> careerCupCompanies = null;

        private static List<string> GetCareerCupCompanies()
        {
            if (careerCupCompanies == null)
            {
                careerCupCompanies = new List<string>();

                Debug("Wait one moment, while I query for companies...");
                PhantomJSDriver driver = InitPhantomJS();

                try
                {
                    Debug("Logging into CareerCup website...");
                    driver.Navigate().GoToUrl(website);

                    Debug("Searching for companies...");
                    IWebElement questionsLink = driver.FindElement(By.LinkText("Questions"));
                    questionsLink.Click();

                    var companyCombo = driver.FindElement(By.Id("company"));
                    var options = companyCombo.FindElements(By.TagName("option"));

                    if (options == null || options.Count == 0)
                    {
                        throw new InvalidOperationException("Error: Unable to find list of companies!");
                    }

                    Debug("Successfully Retrieved companies");
                    Console.WriteLine();

                    foreach (var o in options)
                    {
                        careerCupCompanies.Add(o.Text);
                    }
                }
                finally
                {
                    driver.Quit();
                }
            }
            return careerCupCompanies;
        }

        private static PhantomJSDriver InitPhantomJS()
        {

            PhantomJSDriverService service = PhantomJSDriverService.CreateDefaultService();
            service.LogFile = @".\phantom.log";
            service.SuppressInitialDiagnosticInformation = true;
            int firstRow = Console.CursorTop;
            //var tmp = Console.Out;
            //Console.SetOut(new StringWriter(new StringBuilder()));
            PhantomJSDriver driver = new PhantomJSDriver(service);
            //Console.SetOut(tmp);
            ClearConsole(firstRow);

            /* DOESN'T CLEAR CONSOLE!!!!! AGRAVATING!!!!!
            PhantomJSOptions ops = new PhantomJSOptions();
            var logTypes = new List<string>()
            {
                LogType.Browser, LogType.Client, LogType.Driver, LogType.Profiler, LogType.Server
            };
            foreach (var logType in logTypes)
            {
                ops.SetLoggingPreference(logType, LogLevel.Off);
            }

            ops.AddAdditionalCapability("phantomjs.cli.args", "--webdriver-loglevel=NONE");
            PhantomJSDriver driver = new PhantomJSDriver(service, ops);
            */

            /* // USED AS A REFERENCE
            DesiredCapabilities dcap = new DesiredCapabilities();
            String[] phantomArgs = new String[] {
    "--webdriver-loglevel=NONE"
};
            dcap.SetCapability("phantomjs.cli.args", phantomArgs);


            
            PhantomJSDriver phantomDriver = new PhantomJSDriver(dcap);*/





            return driver;
        }

        private static void ClearConsole(int firstRow)
        {
            int lastRow = Console.CursorTop;
            int count = lastRow - firstRow + 1;
            for (int i = 0; i < count; i++)
            //while(Console.CursorTop>=stop)
            {
                // Start at beginning of current row
                Console.SetCursorPosition(0, Console.CursorTop - i);

                // Blank out/Clear current row
                Console.Write(new string(' ', Console.BufferWidth));
                //Console.Write(new string(' ', Console.WindowWidth));


                Console.SetCursorPosition(0, lastRow);
            }
            Console.SetCursorPosition(0, Console.CursorTop - count + 1);
        }

        private static Option PreviousMenu(Menu prevMenu)
        {
            return new Option(
                    "Back to previous menu",
                    "b",
                    () => { GoBack(prevMenu); });
        }

        /*
        private static OptionsResult ShowMenu(Menu menu)
        {
            return ShowMenu(menu.MenuType, menu.Previous);
        }
        */

        private static void GoBack(Menu prevMenu)
        {
            ShowMenu(prevMenu);
        }

        private static void ClearSearchTerms()
        {
            keywords.Clear();
        }

        private static void GetSearchTerms()
        {
            Console.WriteLine("Search terms:");
            foreach (var k in keywords)
            {
                var quote = "\"{0}\"";
                var quoteLabel = string.Format(quote, k);
                Console.Write("{0} ", quoteLabel);
            }
            Console.WriteLine();
            Console.WriteLine();
        }
        private static void RemoveSearchTerms()
        {
            string entry = Ask("Enter one or more search terms to remove:");
            var tokens = entry.Split(' ', '\t');
            RemoveSearchTerms(tokens);
        }

        private static void RemoveSearchTerms(string[] tokens)
        {
            if (tokens == null || tokens.Length == 0)
            {
                Console.WriteLine("No search terms were entered");
            }
            else
            {
                foreach (var t in tokens)
                {
                    keywords.Remove(t);
                }
            }
        }

        private static void AddSearchTerms()
        {
            string entry = Ask("Enter one or more search terms to add:");
            var tokens = entry.Split(' ', '\t');
            Console.WriteLine();
            AddSearchTerms(tokens);
        }

        private static void AddSearchTerms(string[] tokens)
        {
            if (tokens == null || tokens.Length == 0)
            {
                Console.WriteLine("No search terms were entered");
            }
            else
            {
                foreach (var t in tokens)
                {
                    keywords.Add(t);
                }
            }
        }

        private static void AddSearchTerms(HashSet<string> searchTerms)
        {
            AddSearchTerms(searchTerms.ToArray());
        }

        private static string Ask(string prompt)
        {
            Console.WriteLine(prompt);
            var response = Console.ReadLine();
            return response;
        }

        private static OptionsResult ShowOptions(
            string sentence, params Option[] options)
        {
            return ShowOptions(sentence, true, options);
        }


        private static OptionsResult ShowOptions(
            string sentence, bool autoexecute, params Option[] options)
        {
            var shortcutHash = new Dictionary<string, int>();
            var indexHash = new Dictionary<int, int>();
            if (options == null || options.Length == 0)
            {
                throw new InvalidOperationException(
                    "Error: options must contain 1 or more elements");
            }
            Console.WriteLine(sentence);
            var opNum = 0;
            for (int j = 0; j < options.Length; j++)
            {
                var thisOption = options[j];
                if (thisOption == null) { continue; }
                opNum++;
                shortcutHash.Add(thisOption.DisplayShortcut, j);
                indexHash.Add(opNum, j);
                thisOption.Id = j;
                Console.WriteLine("{0}) {1}", opNum, thisOption);
            }

            if (autoexecute)
            {
                return ExecuteOption(options, shortcutHash, indexHash);
            }
            else
            {
                return new OptionsResult(options, shortcutHash, indexHash);
            }
        }

        private static OptionsResult ExecuteOption(Option[] options, Dictionary<string, int> shortcutHash, Dictionary<int, int> indexHash)
        {
            bool valid = false;
            string chosen = null;
            while (!valid)
            {
                chosen = GetUserInput();
                if (!ExecuteChoice(options, indexHash, shortcutHash, chosen))
                {
                    Console.WriteLine("Invalid selection '{0}'. Chose a number or shorcut.", chosen);
                    continue;
                }
                valid = true;
            }

            return new OptionsResult(options, shortcutHash, indexHash);
        }

        private static string GetUserInput()
        {
            string text;
            Console.Write(":");
            text = Console.ReadLine().Trim();
            Console.WriteLine();
            return text;
        }

        private static bool ExecuteChoice(Option[] options, Dictionary<int, int> indexHash, Dictionary<string, int> shortcutHash, string chosen)
        {
            // See if the user entered a number
            bool found = false;
            var num = GetSelection(true, options, indexHash, shortcutHash, chosen, ref found);
            //return false;
            return found;
        }

        private static int GetSelection(bool autoexecute,
            Option[] options, Dictionary<int, int> indexHash, Dictionary<string, int> shortcutHash, string chosen, ref bool found)
        {
            int num;
            int opIndex = -1;
            found = false;
            if (int.TryParse(chosen, out num))
            {
                if (num > 0 && num <= options.Length)
                {
                    var index = indexHash[num];
                    if (autoexecute)
                    {
                        options[index].Execute();
                    }
                    //return true;
                    found = true;
                    opIndex = index;
                }
                else
                {
                    //return false;
                }
            }

            // See if it's a shortcut that was entered by user
            if (!found)
            {
                foreach (var key in shortcutHash.Keys)
                {
                    if (key.ToLower() == Option.ENTER.ToLower())
                    {
                        if (string.IsNullOrWhiteSpace(chosen))
                        {
                            var index = shortcutHash[key];
                            if (autoexecute)
                            {
                                options[index].Execute();
                            }
                            //return true;
                            found = true;
                            opIndex = index;
                            break;
                        }
                    }
                    if (key == chosen)
                    {
                        var index = shortcutHash[key];
                        if (autoexecute)
                        {
                            options[index].Execute();
                        }
                        //return true;
                        found = true;
                        opIndex = index;
                        break;
                    }
                }
            }

            return opIndex;
        }

        private static void ShowOptions(
            string sentence, params string[] options)
        {
            if (options == null || options.Length == 0)
            {
                throw new InvalidOperationException(
                    "Error: options must contain 1 or more elements");
            }
            Console.WriteLine(sentence);
            for (int j = 1; j <= options.Length; j++)
            {
                Console.WriteLine("{0}) {1}", j, options[j - 1]);
            }
        }

        private static void Prototype()
        {
            string topic = "String Manipulation";
            IWebDriver driver = new ChromeDriver();
            try
            {
                DisplayQuestions(driver, topic);
            }
            finally
            {
                driver.Quit();
            }
        }

        private static bool pause = true;
        private static InterviewData DisplayQuestions(IWebDriver driver, string company, string topic, int pageStart = -1, int qStart = -1)
        {
            var questions = new List<Question>();
            var interviewData = new InterviewData(questions);
            var currentPage = 1;
            var pages = Search(driver, currentPage, company, topic, pageStart);
            foreach (var page in pages)
            {
                /*questions.AddRange(*/Print(interviewData, page.PageNumber, page.Result, company: company, topic: topic, qStart:qStart)/*)*/;
            }
            //return new InterviewData(questions);
            return interviewData;
        }

        private static InterviewData DisplayQuestions(IWebDriver driver, string topic, int pageStart=-1)
        {
            var questions = new List<Question>();
            var interviewData = new InterviewData(questions);
            var currentPage = 1;
            var pages = Search(driver, currentPage, pageStart, topic);
            foreach (var page in pages)
            {
                /*questions.AddRange(*/Print(interviewData, page.PageNumber, page.Result,company:null,topic:topic)/*)*/;
            }
            //return new InterviewData(questions);
            return interviewData;
        }

        private static InterviewData DisplayQuestions(IWebDriver driver, Action setOptions, int pageStart=-1)
        {
            var questions = new List<Question>();
            var interviewData = new InterviewData(questions);
            var currentPage = 1;
            var pages = Search(driver, currentPage, pageStart, setOptions);
            foreach (var page in pages)
            {
                /*questions.AddRange(*/Print(interviewData,page.PageNumber, page.Result, company:null, topic:null)/*)*/;
            }
            //return new InterviewData(questions);
            return interviewData;
        }


        private static void Print(InterviewData interviewData, int pageNum, ReadOnlyCollection<IWebElement> questionsGrid, string company, string topic, int qStart = -1)
        {
            //var links = new List<IWebElement>();
            var qNum = 0; // question<0?0:(question-1);
            var questions = interviewData.Questions;//new List<Question>();
            foreach (var qElem in questionsGrid)
            {
                ++qNum;
                if (qStart > 0 && qNum < qStart) { continue; }
                
                try
                {
                    var question = ExtractQuestion(qElem);
                    if (question == null) { continue; }
                    var found = true;
                    if (keywords != null && keywords.Count > 0)
                    {
                        found = false;
                        foreach (var k in keywords)
                        {
                            if (question.Text.Contains(k))
                            {
                                question.Keywords = new List<string>();
                                question.Keywords.Add(k);
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found) { continue; }
                    question.Company = company;
                    question.Topic = topic;
                    questions.Add(question);
                    /*
                    Console.WriteLine("".PadLeft(Console.BufferWidth, '-'));
                    Console.WriteLine("Question {0}-{1}: {2}", pageNum, qNum, link);
                    Console.WriteLine("".PadLeft(Console.BufferWidth, '-'));
                    Console.WriteLine(question);
                    Console.WriteLine("".PadLeft(Console.BufferWidth, '-'));
                    Console.WriteLine();
                    */
                    var sb = new StringBuilder();
                    var width = Console.WindowWidth;
                    sb.AppendLine("".PadLeft(width, '-'));
                    sb.AppendLine(string.Format("Question {0}-{1}: {2}", pageNum, qNum, question.Url));
                    sb.AppendLine("".PadLeft(width, '-'));
                    sb.AppendLine(question.Text);
                    sb.AppendLine("".PadLeft(width, '-'));
                    sb.AppendLine();
                    var output = sb.ToString();
                    Console.WriteLine(output);
                    using (var sw = new StreamWriter(new FileStream("output.txt", FileMode.Append, FileAccess.Write)))
                    {
                        sw.Write(output);
                        sw.Flush();
                    }

                    var json = JsonConvert.SerializeObject(interviewData);
                    using (var sw = new StreamWriter(new FileStream("output.json", FileMode.Create, FileAccess.Write)))
                    {
                        sw.Write(json);
                        sw.Flush();
                    }

                    if (pause)
                    {
                        //Console.WriteLine("Hit enter to continue to next question...");
                        var list = new List<Option>();
                        Func<OptionsResult> show = () =>
                        {
                            var r = ShowOptions(
                            "",
                            false,
                            list.ToArray()
                            );

                            r.chosen = GetUserInput();
                            var index = GetSelection(false, r.options, r.indexHash, r.shortcutHash, r.chosen, ref found);
                            var selected = r.options[index];
                            selected.Execute();
                            return r;
                        };
                        var myMenu = new CustomMenu(list, show, null);
                        list.AddRange(ListOptions(new Menu(MenuType.Keywords, myMenu)));

                        list.Insert(0,
                            new ValueOption<string>("Hit enter to continue to next question...", "",
                            () =>
                            {
                                // Print next question 
                                return "Enter";
                            })
                            );

                        list.Insert(0,
                            new Option("Remove pause", "u",
                            () =>
                            {
                                pause = false;
                            })
                            );

                        myMenu.Show();
                    }
                }
                catch (Exception ex)
                {
                    var errSB = new StringBuilder();
                    errSB.AppendFormat("Error encountered while extracting/printing question Page {0} - {0}.", qNum).AppendLine();
                    errSB.AppendFormat("Topic {0}:", selectedTopic).AppendLine();
                    errSB.AppendFormat("Company {0}:", selectedCompany).AppendLine();
                    errSB.AppendFormat("Keywords: ").AppendLine();
                    foreach (var k in keywords)
                    {
                        errSB.AppendLine(k);
                    }
                    errSB.AppendFormat("Exception:\r\n", ex.ToString()).AppendLine();
                    errSB.AppendLine();

                    Console.WriteLine(errSB.ToString());
                    using (var sw = new StreamWriter(new FileStream("error.log", FileMode.Append, FileAccess.Write)))
                    {
                        sw.Write(errSB.ToString());
                        sw.Flush();
                    }
                }
            }
            //Console.WriteLine("No more questions");
            //return questions;
        }

        private static Question ExtractQuestion(IWebElement qElem)
        {
            var entry = qElem.FindElement(By.ClassName("entry"));
            var link = entry.FindElement(By.TagName("a"));
            var url = link.GetAttribute("href");
            var pList = entry.FindElements(By.TagName("p"));
            
            string qText = null;
            if (pList == null || pList.Count == 0)
            {
                Console.WriteLine("Error: No <p> found in anchor tag for question. Therefore the question may not have any text.");
                Console.WriteLine("Let's try the text field for the anchor tag");
                qText = link.Text;
                if (string.IsNullOrWhiteSpace(qText))
                {
                    Console.WriteLine("Error: No text found for link....So question will be skipped");
                    return null;
                }
                else
                {
                    //return text;
                }
            }
            else
            {
                var p = pList[0];
                qText = p.Text;
                //return text;
            }

            List<string> tags = GetTags(qElem);
            return new Question(qText, url, tags);
        }

        private static List<string> GetTags(IWebElement qElem)
        {
            var tElem = qElem.FindElement(By.ClassName("tags"));
            var tags = new List<string>();
            foreach (var a in tElem.FindElements(By.TagName("a")))
            {
                tags.Add(a.GetAttribute("text"));
            }

            return tags;
        }

        private static IEnumerable<PageResult> Search(IWebDriver driver, int currentPage, string company, string topic, int pageStart = -1)
        {
            return Search(driver, currentPage, pageStart, () =>
            {
                SelectCompany(driver, company);
                SelectTopic(driver, topic);
            });
        }

        private static IEnumerable<PageResult> Search(IWebDriver driver, int currentPage, int pageStart, string topic)
        {
            return Search(driver, currentPage, pageStart, () =>
            {
                SelectTopic(driver, topic);
            });
        }

        public class PageResult
        {
            public PageResult(int pageNum, ReadOnlyCollection<IWebElement> result)
            {
                this.PageNumber = pageNum;
                this.Result = result;
            }
            public ReadOnlyCollection<IWebElement> Result { get; set; }
            public int PageNumber { get; set; }
        }

        private static IEnumerable<PageResult> Search(IWebDriver driver, int currentPage, int pageStart, Action setOptions)
        {
            driver.Navigate().GoToUrl(website);
            IWebElement questionsLink = driver.FindElement(By.LinkText("Questions"));
            questionsLink.Click();
            setOptions();
            driver.FindElement(By.XPath("//input[@value='Go']")).Click();

            if (pageStart > 0 && currentPage < pageStart)
            {
                Console.WriteLine("Searching for page {0}...", pageStart);
                IWebElement page = FindPage(driver, pageStart);
                currentPage = pageStart;
            }
            while (true)
            {
                Console.WriteLine("".PadLeft(150, 'x'));
                Console.WriteLine("Page: {0}", currentPage);
                Console.WriteLine("".PadLeft(150, 'x'));
                Console.WriteLine();

                yield return GetQuestions(driver, currentPage);
                IWebElement page = GetNextPage(driver, currentPage++);
                if (page == null) { break; }
                page.Click();
            }
        }

        private static IWebElement GetNextPage(IWebDriver driver, int currentPage)
        {
            var pagesPanels = driver.FindElements(By.ClassName("pageListSection"));

            foreach (var panel in pagesPanels)
            {
                var label = panel.FindElement(By.ClassName("pageListSectionLabel"));
                if (!label.Text.ToLower().Trim().StartsWith("page:")) { continue; }
                var content = panel.FindElement(By.ClassName("pageListSectionContent"));
                var pages = content.FindElements(By.TagName("a"));
                foreach (var p in pages)
                {
                    var pageStr = p.Text.Trim();
                    if (pageStr == "<<" || pageStr == "«") { continue; }
                    if (pageStr == ">>" || pageStr == "»")
                    {
                        return p;
                    }
                    else
                    {
                        var pageNum = int.Parse(pageStr);
                        if (pageNum > currentPage)
                        {
                            return p;
                        }
                    }
                }
            }
            return null;
        }

        private static IWebElement FindPage(IWebDriver driver, int targetPageNum)
        {
            var pagesPanels = driver.FindElements(By.ClassName("pageListSection"));

            foreach (var panel in pagesPanels)
            {
                var label = panel.FindElement(By.ClassName("pageListSectionLabel"));
                if (!label.Text.ToLower().Trim().StartsWith("page:")) { continue; }
                var content = panel.FindElement(By.ClassName("pageListSectionContent"));
                var pages = content.FindElements(By.TagName("a"));

                if (pages.Count == 0) { throw new InvalidOperationException("Error: No pages available!"); }

                bool found = false;
                IWebElement lastPage;
                int lastIndex = -1;
                int lastPageNum = -1;
                for (var back = pages.Count - 1; back >= 0; back--)
                {
                    lastPage = pages[back];
                    var pStr = lastPage.Text.Trim();

                    var isNum = int.TryParse(pStr, out lastPageNum);
                    if (isNum)
                    {
                        found = true;
                        lastIndex = back;
                        break;
                    }
                }

                if (!found) { throw new InvalidOperationException("Error: Could not locate last page!"); }

                Console.WriteLine("Current page set's last page is {0}...", lastPageNum);
                if (targetPageNum < lastPageNum)
                {                    
                    var rewind = lastPageNum - targetPageNum;
                    var targetIndex = lastIndex - rewind;
                    if (targetIndex < 0)
                    {                        
                        return FindBack(driver, targetPageNum, pages);
                    }
                    else
                    {
                        return GetPage(driver, targetPageNum, pages, targetIndex, true);
                    }
                }
                else
                {
                    var fastForward = targetPageNum - lastPageNum;
                    var targetIndex = lastIndex + fastForward;
                    if (targetIndex >= pages.Count)
                    {
                        return FindForward(driver, targetPageNum, pages);
                    }
                    else
                    {
                        return GetPage(driver, targetPageNum, pages, targetIndex, false);
                    }
                }              
            }
            return null;
        }

        private static IWebElement FindForward(IWebDriver driver, int targetPageNum, ReadOnlyCollection<IWebElement> pages)
        {
            Console.WriteLine("Fast-forward for page {0}...", targetPageNum);
            bool found = false;
            for (int j=pages.Count-1; j>=0; j--)
            {
                var p = pages[j];
                var pageStr = p.Text.Trim();
                if (pageStr == ">>" || pageStr == "»") { found = true; p.Click(); return FindPage(driver, targetPageNum); /*break;*/ }
            }
            if (!found) { throw new InvalidOperationException("Error: Could not find << in pages section!"); }

            return null;
        }

        private static IWebElement GetPage(IWebDriver driver, int targetPageNum, ReadOnlyCollection<IWebElement> pages, int targetIndex, bool goBack)
        {
            Console.WriteLine("Page {0} should be in current set of pages at index {1}...", targetPageNum, targetIndex);
            var p = pages[targetIndex];
            var pageStr = p.Text.Trim();
            var pageNum = -1;
            bool isNum = int.TryParse(pageStr, out pageNum);
            if (!isNum)
            {
                Console.WriteLine("Target index {0} in current page set is not a number, it's '{1}'...", targetIndex, pageStr);
                Console.WriteLine("To correct this, will either move forward or back a page set...");
                if (goBack)
                {
                    return FindBack(driver, targetPageNum, pages);
                }
                else
                {
                    return FindForward(driver, targetPageNum, pages);
                }
            }
            else
            {
                if (pageNum == targetPageNum)
                {
                    Console.WriteLine("SUCCESS: Able to locate page {0}", targetPageNum);
                    return p;
                }
                else
                {
                    throw new InvalidOperationException(
                        string.Format("Catastrophic Error: Expected page {0}, Actual {1}", targetPageNum, pageNum));
                }

            }
        }

        private static IWebElement FindBack(IWebDriver driver, int targetPageNum, ReadOnlyCollection<IWebElement> pages)
        {
            Console.WriteLine("Rewind back for page {0}...", targetPageNum);
            bool found = false;
            foreach (var p in pages)
            {
                var pageStr = p.Text.Trim();
                if (pageStr == "<<" || pageStr == "«") { found = true; p.Click(); return FindPage(driver, targetPageNum); /*break;*/ }
            }
            if (!found) { throw new InvalidOperationException("Error: Could not find << in pages section!"); }

            return null;
        }

        private static PageResult GetQuestions(IWebDriver driver, int pageNum)
        {
            var questionsGrid = driver.FindElements(By.ClassName("question"));
            return new PageResult(pageNum, questionsGrid);
        }
        /*
        private static IEnumerable<ReadOnlyCollection<IWebElement>> VisitPages(IWebDriver driver,int currentPage)
        {
            var pagesPanels = driver.FindElements(By.ClassName("pageListSection"));

            foreach (var panel in pagesPanels)
            {
                var label = panel.FindElement(By.ClassName("pageListSectionLabel"));
                if (!label.Text.ToLower().Trim().StartsWith("page:")) { continue; }
                var content = panel.FindElement(By.ClassName("pageListSectionContent"));
                var pages = content.FindElements(By.TagName("a"));
                foreach (var p in pages)
                {
                    var pageStr = p.Text.Trim();
                    if (pageStr == ">>")
                    {
                        p.Click();

                        foreach(var m in Next(driver,currentPage+1))
                        {
                            yield return m;
                        }
                        break;
                    }
                    else
                    {
                        var pageNum = int.Parse(pageStr);
                        if (pageNum > currentPage)
                        {                           
                            p.Click();
                            foreach (var m in Next(driver, pageNum))
                            {
                                yield return m;
                            }
                            break;
                        }
                        break;
                    }
                }
            }
        }
    */
        private static void SelectTopic(IWebDriver driver, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic)) { return; }
            var topicSelect = new SelectElement(driver.FindElement(By.Id("topic")));
            topicSelect.SelectByText(topic);
        }

        private static void SelectCompany(IWebDriver driver, string company)
        {
            if (string.IsNullOrWhiteSpace(company)) { return; }
            var companySelect = new SelectElement(driver.FindElement(By.Id("company")));
            companySelect.SelectByText(company);
        }
    }
}
