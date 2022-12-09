using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

/**
 * Cefsharp Example
 * 
 * BabyBear Project
 * 12/09/2022
 */
namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

        // Initial Url
        private string initUrl = "https://www.imoradar24.ro/apartamente-de-vanzare/iasi-iasi";
        // Map for store reg exp
        private Dictionary<string, string> regExpMap = new Dictionary<string, string>();
        // for store html of current url
        private string html = string.Empty;
        // Embeded browser
        public ChromiumWebBrowser browser;

        /**
         * Initialize Browser
         */
        public void InitBrowser()
        {
            Cef.Initialize(new CefSettings());
            browser = new ChromiumWebBrowser(initUrl);
            splitContainer1.Panel2.Controls.Add(browser);
            splitContainer1.Dock = DockStyle.Fill;

            browser.FrameLoadEnd += WebBrowserFrameLoadEnded;


            textbox_url.Text = initUrl;
        }

        /**
         * Add EventListener for get html when end load
         */
        private void WebBrowserFrameLoadEnded(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                browser.ViewSource();
                browser.GetSourceAsync().ContinueWith(taskHtml =>
                {
                    html = taskHtml.Result;
                });
            }
        }

        /**
         * Load Browser
         */
        public void LoadBrowser(string url)
        {
            browser.Load(url);
        }

        /**
         * Initialize Windows Form
         */
        public Form1()
        {
            InitializeComponent();

            InitBrowser();
        }

        /**
         * Go button / Click Event
         */
        private void button_go_Click(object sender, EventArgs e)
        {
            string url = textbox_url.Text;

            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            LoadBrowser(url);
        }

        /**
         * Import RegExp button / Click Event
         */
        private void button_regexp_Click(object sender, EventArgs e)
        {
            checkboxlist_regExpList.Items.Clear();

            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                //Read the contents of the file into a stream
                var fileContent = string.Empty;
                var fileStream = openFileDialog1.OpenFile();
                

                using (StreamReader reader = new StreamReader(fileStream))
                {
                    // Import RegExp list
                    while (!reader.EndOfStream)
                    {
                        string temp = reader.ReadLine();

                        if (temp.IndexOf("-->") > 0)
                        {
                            checkboxlist_regExpList.Items.Add(temp);
                        }
                    }
                }


            }
            else
            {
                //Operation aborted by the user
            }
        }

        /**
         * Extract RegExp button / Click Event
         */
        private void button_extract_Click(object sender, EventArgs e)
        {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("#"));

            HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

            htmlDoc.OptionFixNestedTags = true;
            htmlDoc.OptionAutoCloseOnEnd = true;
            htmlDoc.OptionCheckSyntax = false;

            HtmlAgilityPack.HtmlNode.ElementsFlags.Remove("option");

            htmlDoc.LoadHtml(html);

            if (htmlDoc.ParseErrors != null && htmlDoc.ParseErrors.Count() > 0)
            {
                // Handle any parse errors as required
                return;
            }
            else
            {

                if (htmlDoc.DocumentNode != null)
                {

                    foreach (string item in checkboxlist_regExpList.CheckedItems)
                    {
                        if (item.IndexOf("-->") > 0)
                        {
                            //dataGridView1.Columns[index].Name = item.Substring(0, item.IndexOf("-->"));

                            table.Columns.Add(new DataColumn(item.Substring(0, item.IndexOf("-->"))));

                            // Get reg exp data from checkbox list
                            String selector = item.Substring(item.IndexOf("-->") + 3).Trim();

                            HtmlAgilityPack.HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(selector);

                            int count = 1;
                            if (nodes.Count > 0)
                            {
                                // Do something with bodyNode
                                foreach(var node in nodes)
                                {
                                    string link = node.GetAttributeValue("href", "default");

                                    if (string.IsNullOrEmpty(link) || link.Equals("#"))
                                    {
                                        continue;
                                    }

                                    table.Rows.Add(new object[] { count ++, link });
                                }
                            }

                            //// convert string to regexp
                            //Regex rg = new Regex(selector, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                            //MatchCollection matchedAuthors = rg.Matches(html);


                            //for (int count = 0; count < matchedAuthors.Count; count++)
                            //{
                            //    //Console.WriteLine(matchedAuthors[count].Value);

                            //    if (string.IsNullOrEmpty(matchedAuthors[count].Value))
                            //    {
                            //        continue;
                            //    }

                            //    table.Rows.Add(new object[] { count + 1, matchedAuthors[count].Value });
                            //}
                        }
                    }

                    
                }
            }

            dataGridView1.DataSource = table;

            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
        }
    }
}
