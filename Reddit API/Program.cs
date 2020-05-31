
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace RedditScraper
{
    public class Program
    {
        static readonly string connectionString = ConfigurationManager.ConnectionStrings["dbConnection"].ToString();
        static DAL dal = new DAL(connectionString);

        static readonly string redditId = ConfigurationManager.AppSettings["redditId"].ToString();
        static readonly string redditSecret = ConfigurationManager.AppSettings["redditSecret"].ToString();

        public static Token Token { get; set; }

        static async Task Main(string[] args)
        {
            await GetAuth();
            await RunIt();
        }

        static public async Task GetAuth()
        {
            string tokenRequestUrl = "https://www.reddit.com/api/v1/access_token";

            HttpClient hc = new HttpClient();

            var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", redditId, redditSecret));
            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            hc.DefaultRequestHeaders.Authorization = header;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, tokenRequestUrl);
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"grant_type", "password" },
                {"username", "YourUsername" },
                {"password", "YourPassword" },
            });

            var response = await hc.SendAsync(request);
            var tokenObject = await response.Content.ReadAsStringAsync();
            Token = JsonConvert.DeserializeObject<Token>(tokenObject);
        }

        static public async Task RunIt()
        {
            try
            {
                string oauthUrl = "https://oauth.reddit.com/r/";

                HttpClient hc = new HttpClient();
                hc.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.AccessToken);
                hc.DefaultRequestHeaders.Add("User-Agent", "UserAgent1");

                // All set at this point. Below is something I whipped up as an example; not exactly the best looking code...

                string subreddits = ConfigurationManager.AppSettings["subreddits"].ToString();

                foreach (string subreddit in subreddits.Split(','))
                {
                    string fullUrl = oauthUrl + subreddit;
                    var response = await hc.GetAsync(fullUrl);
                    var subredditText = await response.Content.ReadAsStringAsync();
                    var json = JsonConvert.DeserializeObject<RootObject>(subredditText);

                    foreach (var article in json.data.children)
                    {
                        RedditPost ra = new RedditPost
                        {
                            PostId = article.data.id,
                            Title = article.data.title,
                            Subreddit = article.data.subreddit,
                            NumberOfComments = article.data.num_comments,
                            NumberOfUpvotes = article.data.ups,
                            IsGilded = Utility.NumberOfGildings(article.data.gildings),
                            PostDate = Utility.GetDateTimeFromRedditUnixTimestamp(article.data.created_utc),
                            Url = article.data.url
                        };

                        if (!dal.PostAlreadyExists(ra.PostId))
                            dal.InsertNewPostToDatabase(ra);
                        else
                            dal.UpdateExistingPosts(ra);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }       

        /* The classes below are from json to C# site and used to parse the response on line 77 */
        public class MediaEmbed
        {
        }

        public class SecureMediaEmbed
        {
        }

        public class Gildings
        {
            public int gid_1 { get; set; }
            public int gid_2 { get; set; }
            public int? gid_3 { get; set; }
        }

        public class Data2
        {
            public object approved_at_utc { get; set; }
            public string subreddit { get; set; }
            public string selftext { get; set; }
            public string author_fullname { get; set; }
            public bool saved { get; set; }
            public object mod_reason_title { get; set; }
            public int gilded { get; set; }
            public bool clicked { get; set; }
            public string title { get; set; }
            public List<object> link_flair_richtext { get; set; }
            public string subreddit_name_prefixed { get; set; }
            public bool hidden { get; set; }
            public object pwls { get; set; }
            public object link_flair_css_class { get; set; }
            public int downs { get; set; }
            public bool hide_score { get; set; }
            public string name { get; set; }
            public bool quarantine { get; set; }
            public string link_flair_text_color { get; set; }
            public string author_flair_background_color { get; set; }
            public string subreddit_type { get; set; }
            public int ups { get; set; }
            public int total_awards_received { get; set; }
            public MediaEmbed media_embed { get; set; }
            public string author_flair_template_id { get; set; }
            public bool is_original_content { get; set; }
            public List<object> user_reports { get; set; }
            public object secure_media { get; set; }
            public bool is_reddit_media_domain { get; set; }
            public bool is_meta { get; set; }
            public object category { get; set; }
            public SecureMediaEmbed secure_media_embed { get; set; }
            public object link_flair_text { get; set; }
            public bool can_mod_post { get; set; }
            public int score { get; set; }
            public object approved_by { get; set; }
            public string thumbnail { get; set; }
            public object edited { get; set; }
            public string author_flair_css_class { get; set; }
            public List<object> author_flair_richtext { get; set; }
            public Gildings gildings { get; set; }
            public object content_categories { get; set; }
            public bool is_self { get; set; }
            public object mod_note { get; set; }
            public double created { get; set; }
            public string link_flair_type { get; set; }
            public object wls { get; set; }
            public object banned_by { get; set; }
            public string author_flair_type { get; set; }
            public string domain { get; set; }
            public bool allow_live_comments { get; set; }
            public string selftext_html { get; set; }
            public object likes { get; set; }
            public object suggested_sort { get; set; }
            public object banned_at_utc { get; set; }
            public object view_count { get; set; }
            public bool archived { get; set; }
            public bool no_follow { get; set; }
            public bool is_crosspostable { get; set; }
            public bool pinned { get; set; }
            public bool over_18 { get; set; }
            public List<object> all_awardings { get; set; }
            public bool media_only { get; set; }
            public bool can_gild { get; set; }
            public bool spoiler { get; set; }
            public bool locked { get; set; }
            public string author_flair_text { get; set; }
            public bool visited { get; set; }
            public object num_reports { get; set; }
            public string distinguished { get; set; }
            public string subreddit_id { get; set; }
            public object mod_reason_by { get; set; }
            public object removal_reason { get; set; }
            public string link_flair_background_color { get; set; }
            public string id { get; set; }
            public bool is_robot_indexable { get; set; }
            public object report_reasons { get; set; }
            public string author { get; set; }
            public int num_crossposts { get; set; }
            public int num_comments { get; set; }
            public bool send_replies { get; set; }
            public object whitelist_status { get; set; }
            public bool contest_mode { get; set; }
            public List<object> mod_reports { get; set; }
            public bool author_patreon_flair { get; set; }
            public string author_flair_text_color { get; set; }
            public string permalink { get; set; }
            public object parent_whitelist_status { get; set; }
            public bool stickied { get; set; }
            public string url { get; set; }
            public int subreddit_subscribers { get; set; }
            public double created_utc { get; set; }
            public object discussion_type { get; set; }
            public object media { get; set; }
            public bool is_video { get; set; }
        }

        public class Child
        {
            public string kind { get; set; }
            public Data2 data { get; set; }
        }

        public class Data
        {
            public string modhash { get; set; }
            public int dist { get; set; }
            public List<Child> children { get; set; }
            public string after { get; set; }
            public object before { get; set; }
        }

        public class RootObject
        {
            public string kind { get; set; }
            public Data data { get; set; }
        }
    }
}