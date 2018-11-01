using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using System.IO;

using System.Web;

namespace MetalBot.Helpers
{
    public class MessageUtil
    {
        const string subscriptionKey = "<YOUR VISION API KEY>";
        const string uriBase = "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/ocr";

        public static string GetGreeting()
        {
            int idx = new Random(Guid.NewGuid().GetHashCode()).Next(-0, Greetings.Count());
            return Greetings[idx];
        }
        public static List<string> GetPositiveResponses(int nResponses)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            HashSet<string> Responses = new HashSet<string>();
            for (int i = 0; i < nResponses; i++)
            {
                int idx = r.Next(-0, PositiveResponeses.Count());
                Responses.Add(PositiveResponeses[idx]);
            }
            return Responses.ToList();
        }
        public static async Task<string> DetectTextInImageAsync(string contentURL)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", subscriptionKey);

                string requestParameters = "language=unk&detectOrientation=true";
                string uri = uriBase + "?" + requestParameters;

                HttpResponseMessage response;
                byte[] imageBytes;

                using (var webClient = new WebClient())
                {
                    imageBytes = webClient.DownloadData(contentURL);
                }

                using (ByteArrayContent content = new ByteArrayContent(imageBytes))
                {
                    content.Headers.ContentType =
                        new MediaTypeHeaderValue("application/octet-stream");

                    response = await client.PostAsync(uri, content);
                }

                string contentString = await response.Content.ReadAsStringAsync();

                return contentString ;
            }
            catch (Exception e)
            {
                return "Unknown";
            }
        }

        public static Dictionary<Drink, int> FindDrinkInLabelText(string LabelText)
        {
            Dictionary<Drink, int> results = new Dictionary<Drink, int>();
            Dictionary<Drink, string[]> KeywordMappings = new Dictionary<Drink, string[]>();

            KeywordMappings[Drink.WHISKY]   = new string[] { "Whisky", "Whiskey", "Single Malt", "Scotch", "Tennessee" };
            KeywordMappings[Drink.BEER]     = new string[] { "Bier", "Beer", "Weisse", "Pils", "Pilsner" };
            KeywordMappings[Drink.GIN]      = new string[] { "Gin", "Botanist", "London" };
            KeywordMappings[Drink.RUM]      = new string[] { "Rum", "Ron", "Blanca", "Bacardi", "Havana" };
            KeywordMappings[Drink.VODKA]    = new string[] { "Vodka" };
            KeywordMappings[Drink.JUICE]    = new string[] { "Saft", "Apfel", "Orange" };
            KeywordMappings[Drink.WATER]    = new string[] { "Wasser", "Water", "Mineral" };

            foreach (KeyValuePair<Drink, string[]> kvp in KeywordMappings)
            {
                foreach (string keyword in kvp.Value)
                {
                    if (LabelText.ToLower().Contains(keyword.ToLower()))
                    {
                        if (results.ContainsKey(kvp.Key))
                        {
                            results[kvp.Key] += 1;
                        }
                        else
                        {
                            results[kvp.Key] = 1;
                        }
                    }
                }
            }
            return results;
        }

        public static string GetBandForStyle(Style s)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            int idx = 0;
            switch (s)
            {
                case Style.BLACK_METAL:
                    idx = r.Next(-0, BlackMetalBands.Count());
                    return BlackMetalBands[idx];
                case Style.DEATH_METAL:
                    idx = r.Next(-0, DeathMetalBands.Count());
                    return DeathMetalBands[idx];
                case Style.GLAM_ROCK:
                    idx = r.Next(-0, GlamRockBands.Count());
                    return GlamRockBands[idx];
                case Style.GOTHIC_METAL:
                    idx = r.Next(-0, GothicMetalBands.Count());
                    return GothicMetalBands[idx];
                case Style.HARDROCK:
                    idx = r.Next(-0, HardrockBands.Count());
                    return HardrockBands[idx];
                case Style.HEAVY_METAL:
                    idx = r.Next(-0, HeavyMetallBands.Count());
                    return HeavyMetallBands[idx];
                case Style.POP:
                    idx = r.Next(-0, PopBands.Count());
                    return PopBands[idx];
                case Style.SRAIGHT_X:
                    idx = r.Next(-0, StraightXBands.Count());
                    return StraightXBands[idx];
                case Style.THRASH_METAL:
                    idx = r.Next(-0, ThrashMetalBands.Count());
                    return ThrashMetalBands[idx];
                default:
                    return "Vermutlich Radio";
            }
        }

        public static string StyleToString(Style s)
        {
            switch(s)
            {
                case Style.BLACK_METAL:
                    return "Black Metal";
                case Style.DEATH_METAL:
                    return "Death Metal";
                case Style.GLAM_ROCK:
                    return "Glam Rock";
                case Style.GOTHIC_METAL:
                    return "Gothic Metal";
                case Style.HARDROCK:
                    return "Hardrock";
                case Style.HEAVY_METAL:
                    return "Heavy Metal";
                case Style.POP:
                    return "Pop :-(";
                case Style.SRAIGHT_X:
                    return "Straight Edge Musik";
                case Style.THRASH_METAL:
                    return "Thrash Metal";
                default:
                    return "etwas anderes";
            }
        }

        public static Style StringToStyle(String s)
        {
            switch (s)
            {
                case "Black Metal":
                    return Style.BLACK_METAL;
                case "Death Metal":
                    return Style.DEATH_METAL;
                case "Glam Rock":
                    return Style.GLAM_ROCK;
                case "Gothic Metal":
                    return Style.GOTHIC_METAL;
                case "Hardrock":
                    return Style.HARDROCK;
                case "Heavy Metal":
                    return Style.HEAVY_METAL;
                case "Pop :-(":
                    return Style.POP;
                case "Straight Edge Musik":
                    return Style.SRAIGHT_X;
                case "Thrash Metal":
                    return Style.THRASH_METAL;
                default:
                    return Style.UNKNOWN;
            }
        }

        public static string DrinkToString(Drink d)
        {
            switch (d)
            {
                case Drink.BEER:
                    return "Bier";
                case Drink.GIN:
                    return "Gin";
                case Drink.JUICE:
                    return "Saft";
                case Drink.RUM:
                    return "Rum";
                case Drink.VODKA:
                    return "Vodka";
                case Drink.WATER:
                    return "Wasser";
                case Drink.WHISKY:
                    return "Whisky";
                case Drink.WINE:
                    return "Wein";
                default:
                    return "etwas anderes";
            }
        }

        public static List<Style> GetStylesForDrink(Drink d, int MaxStyles = 3)
        {
            List<Style> rv = new List<Style>();
            switch (d)
            {
                case Drink.BEER:
                    rv.Add(Style.THRASH_METAL);
                    rv.Add(Style.HEAVY_METAL);
                    rv.Add(Style.DEATH_METAL);
                    rv.Add(Style.HARDROCK);
                    break;
                case Drink.GIN:
                    rv.Add(Style.HARDROCK);
                    rv.Add(Style.HEAVY_METAL);
                    rv.Add(Style.GLAM_ROCK);
                    break;
                case Drink.JUICE:
                    rv.Add(Style.POP);
                    break;
                case Drink.RUM:
                    rv.Add(Style.GLAM_ROCK);
                    rv.Add(Style.HARDROCK);
                    rv.Add(Style.HEAVY_METAL);
                    break;
                case Drink.VODKA:
                    rv.Add(Style.BLACK_METAL);
                    rv.Add(Style.DEATH_METAL);
                    rv.Add(Style.GLAM_ROCK);
                    break;
                case Drink.WATER:
                    rv.Add(Style.SRAIGHT_X);
                    break;
                case Drink.WHISKY:
                    rv.Add(Style.HARDROCK);
                    rv.Add(Style.HEAVY_METAL);
                    rv.Add(Style.DEATH_METAL);
                    break;
                case Drink.WINE:
                    rv.Add(Style.GOTHIC_METAL);
                    break;
                default:
                    rv.Add(Style.UNKNOWN);
                    break;
            }
            return rv.Take(MaxStyles).ToList();
        }

        static string[] Greetings           = { "Hi", "Infernal hails", "Sei gegrüßt", "Up the irons", "Gut mosh", "Prost" };
        static string[] PositiveResponeses  = { "Ja" , "Yeah", "Hell Yeah", "Verdammt nochmal, ja!", "Aber klar doch", "Was denkst Du denn?", "Aber sowas von!", "Wer, wenn nicht ich?", "Darauf habe ich schon immer gewartet", "Logo", "Mach los!", "Let's go"};
        static string[] GothicMetalBands    = { "Type O Negative" };
        static string[] HeavyMetallBands    = { "Iron Maiden", "Powerwolf", "Iced Earth", "Blind Guardian" };
        static string[] ThrashMetalBands    = { "Anthrax", "Slayer", "Metallica", "Pantera", "Megadeth" };
        static string[] PopBands            = { "Justin Bieber" };
        static string[] BlackMetalBands     = { "Darkthrone", "Der Weg einer Freiheit", "Satyricon", "Emperor" };
        static string[] GlamRockBands       = { "Poison", "Motley Crue", "Steel Panther" };
        static string[] HardrockBands       = { "Motörhead", "Volbeat", "AC/DC", "Airbourne" };
        static string[] StraightXBands      = { "7 Seconds", "Minor Threat", "Youth Of Today" };
        static string[] DeathMetalBands     = { "Ulcerate", "Morbid Angel", "Unleashed" };
    }
    public enum Drink { BEER, WINE, WHISKY, JUICE, GIN, VODKA, WATER, RUM }
    public enum Style { GOTHIC_METAL, THRASH_METAL, POP, DEATH_METAL, BLACK_METAL, HEAVY_METAL, GLAM_ROCK, HARDROCK, SRAIGHT_X, UNKNOWN }
    public enum State { INITIATED, IMAGE_TRIGGERED, AWAITING_IMAGE, GIVING_RECOMMENDATION, FINDING_BANDS }

}