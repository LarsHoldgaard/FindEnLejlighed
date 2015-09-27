using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using FindEnLejlighed.Services.Database;
using FindEnLejlighed.Services.Domain;
using HtmlAgilityPack;
using Newtonsoft.Json;
using static System.Decimal;

namespace FindEnLejlighed.Services.Services
{
    public class DbaApartmentService : IApartmentService
    {
        private const string BasePath = "http://www.dba.dk/boliger/lejebolig/";
        private const string Paging = "side-";
        private const int PageCount = 5;

        private const string Dba_name = "Martin Carlsson";
        private const string Dba_email = "martin@imus.dk";
        private const string Dba_phone = "42927250";
        private const string Dba_message = "Hej%0D%0A%0D%0AJeg+er+meget+interesseret+i+lejligheden%2C+specielt+er+jeg+vild+med+lejlighedens+placering%2C+kvarteret+og+st%C3%B8rrelse.+%0D%0A%0D%0AJeg+er+en+travl+person.+Jeg+arbejder+det+meste+af+tiden%2C+og+er+derfor+en+meget+nem+lejer%3A+%0D%0A-+Jeg+ryger+ikke%0D%0A-+Jeg+er+dygtig+med+h%C3%A6nderne%2C+og+kan+klare+de+fleste+h%C3%A5ndv%C3%A6rker+opgaver+selv+%0D%0A-+Jeg+holder+lejligheden+ren+og+p%C3%A6n+%0D%0A-+Jeg+har+en+solid+%C3%B8konomi%0D%0A-+Alts%C3%A5%2C+jeg+vil+passe+rigtigt+godt+p%C3%A5+din+lejlighed%0D%0A%0D%0AJeg+er+fleksibel+med+indflytningsdato.%0D%0A%0D%0AVh%0D%0AMartin+Carlsson";

        public List<Apartment> GetApartments()
        {
            List<Apartment> apartments = new List<Apartment>();
            var html = GetBaseHtml(CollectUrls());

            foreach (var singleHtml in html)
            {
                var links = GetApartmentLinksOnPage(singleHtml);

                Parallel.ForEach(links, apartmentLink =>
                {
                    var apartment = ParseApartment(apartmentLink);

                    if (apartment != null)
                    {
                        apartments.Add(apartment);
                        var saved_apartment = SaveApartment(apartment);
                        Send(saved_apartment);
                    }

                });
            }

            return apartments;
        }

        public void Send(Apartment apartment)
        {
            // contact status plus worst case
            if (apartment.ContactStatus != ContactStatus.Contacted && apartment.Link.Contains("-vaer") &&
                !apartment.Link.Contains("1-vaer"))
            {
                if (LegalPostCode(apartment) && 
                    apartment.SellerType == "PRIVATE" && 
                    apartment.ContactStatus == ContactStatus.New &&
                    apartment.Price >= 5000)
                {
                    SendDba(apartment);
                }
            }
        }

        private bool LegalPostCode(Apartment apartment)
        {
            if (string.IsNullOrEmpty(apartment.Postcode))
            {
                return false;
            }

            int tryParse = 0;
            int.TryParse(apartment.Postcode, out tryParse);

            if (tryParse <= 0)
            {
                return false;
            }

            int post = int.Parse(apartment.Postcode);

            if ((post <= 2900) || ((post >= 3050) && (post <= 3060)) || ((post >= 3400) && (post <= 3460)))
            {
                return true;
            }
            return false;
        }

        private void SendDba(Apartment apartment)
        {
            try
            {
                CookieContainer cookies = new CookieContainer();

                HttpWebRequest initial_request = (HttpWebRequest)WebRequest.Create(apartment.Link);
                initial_request.CookieContainer = cookies;
                HttpWebResponse initial_response = (HttpWebResponse)initial_request.GetResponse();

                // Get the stream associated with the response.
                Stream receiveStream = initial_response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var currentHtml = readStream.ReadToEnd();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(currentHtml);

                var input = doc.DocumentNode
                  .Descendants("input")
                  .First(n => n.Attributes["name"].Value == "__RequestVerificationToken").GetAttributeValue("value", "");


                var payload =
                    string.Format(
                        "__RequestVerificationToken={0}&SendEmailAnalyticsLabel=SendEmailPrivate&ExternalListingId={1}&IsCas=False&PayPalEnabledByClassification=False&Name={2}&Email={3}&PhoneNumber={4}&Message={5}&BccToSender=true",
                        input,
                        apartment.DbaId,
                        Dba_name,
                        Dba_email,
                        Dba_phone,
                        Dba_message);


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://dba.dk/ajax/vip/ContactForm/SendEmail");
                request.Method = "POST";
                request.CookieContainer = cookies;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";

                request.Headers.Add("Accept-Language", "en-GB,en;q=0.8,en-US;q=0.6,da;q=0.4");
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");


                byte[] buf = Encoding.UTF8.GetBytes(payload);
                request.ContentLength = buf.Length;

                Stream newStream = request.GetRequestStream();

                newStream.Write(buf, 0, buf.Length);



                using (var context = new Context())
                {
                    var the_apartment = context.Apartments.FirstOrDefault(c => c.Id == apartment.Id);
                    the_apartment.ContactStatus = ContactStatus.Contacted;

                    var send = new SendMessage()
                    {
                        DbaId = apartment.DbaId,
                        DateSent = DateTime.Now,
                        Email = Dba_email,
                        Message = Dba_message,
                        Name = Dba_name,
                        Phone = Dba_phone
                    };
                    context.SendMessages.Add(send);
                    context.SaveChanges();
                }


                var HttpWebResponse = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception)
            {
                Console.WriteLine("Could not sent to ", apartment.Link);
            }
         



        }



        private Apartment SaveApartment(Apartment apartment)
        {
            int id = 0;
            using (var context = new Context())
            {
                if (!context.Apartments.Any(c => c.Link == apartment.Link))
                {
                    context.Apartments.Add(apartment);
                    context.SaveChanges();

                    id = apartment.Id;

                    return context.Apartments.FirstOrDefault(c => c.Id == id);
                }
                else
                {
                    var existing_apartment = context.Apartments.FirstOrDefault(c => c.Link == apartment.Link);
                    return existing_apartment;
                }

                
            }
                
            
        }

        #region Get single apartment link

        private Apartment ParseApartment(string url)
        {
            try
            {
                string html = string.Empty;
                Console.WriteLine("Pulling info from {0}", url);

                using (var client = new WebDownload())
                {
                    html = client.DownloadString(url);

                }
                var apartment = new Apartment()
                {
                    ContactStatus = ContactStatus.New,
                    Link = url,
                    DateCreated = DateTime.Now
                };

                FillApartment(apartment, html, url);

                return apartment;
            }
            catch (Exception)
            {
                Console.WriteLine("Timeout on parse apartment");
                return null;
            }
       

        }

        private void FillApartment(Apartment apartment, string html, string url)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            setPrice(apartment, doc);
            setDatalayerInfo(apartment, doc);


        }

        private void setDatalayerInfo(Apartment apartment, HtmlDocument doc)
        {
            try
            {
                var uglyStart = "var dataLayer = ";
                var uglyEnd = "var dbaContext =";

                var indexOfEnd = doc.DocumentNode.InnerHtml.IndexOf(uglyEnd);
                var indexOfStart = doc.DocumentNode.InnerHtml.IndexOf(uglyStart);
                var length = doc.DocumentNode.InnerHtml.Length;

                // i will admit this code is.... interesting
                var dataLayer = doc.DocumentNode.InnerHtml.Substring(
                    indexOfStart + uglyStart.Length,
                    indexOfEnd - indexOfStart - uglyEnd.Length - 1 - "     ".Length);

                var layer = JsonConvert.DeserializeObject<List<DatalayerHelper>>(dataLayer).FirstOrDefault();

                if (layer != null)
                {
                    apartment.DbaId = layer.a.id;
                    apartment.CityRegion = layer.a.attr.CityRegion;
                    apartment.TakeOverDate = layer.a.attr.TakeoverDate;
                    apartment.SellerType = layer.a.attr.SELLER_TYPE;
                    apartment.Postcode = layer.a.attr.Postcode;
                    apartment.SquareMeter = layer.a.attr.Boligkvm;
                    apartment.Boligkvm = layer.a.attr.Boligkvm;
                    apartment.Deposit = layer.a.attr.Deposit;
                    apartment.partlyfurnished = layer.a.attr.partlyfurnished;
                    apartment.Washingmachine = layer.a.attr.Washingmachine;
                    apartment.basement = layer.a.attr.basement;
                    apartment.minlivingspace = layer.a.attr.minlivingspace;

                }



            }
            catch (Exception ex)
            {
                Console.WriteLine("Datalayer didn't work on ", apartment.Link);
            }


        }

        private void setPrice(Apartment apartment, HtmlDocument doc)
        {

            try
            {
                // price
                var priceClass = "price-tag";
                var priceNode =
                    doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", priceClass)).FirstOrDefault();

                if (priceNode != null)
                {
                    var htmlPrice = priceNode.InnerHtml;
                    var price = htmlPrice.Replace(" kr.", "").Replace(".", "");

                    apartment.Price = Parse(price);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("No price on this one");
            }

        }

        private List<string> GetApartmentLinksOnPage(string html)
        {
            List<string> urls = new List<string>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var listingClass = "dbaListing";

            var listings = doc.DocumentNode.SelectNodes(string.Format("//*[contains(@class,'{0}')]", listingClass));

            foreach (var listing in listings)
            {
                var url = GetUrlOnListing(listing);
                urls.Add(url);
            }

            return urls;
        }

        private string GetUrlOnListing(HtmlNode listingHtml)
        {
            return listingHtml.Descendants("a").FirstOrDefault().GetAttributeValue("href", "");

        }


        #endregion

        #region GetHtml structure

        private List<string> GetBaseHtml(List<string> urls)
        {
            List<string> html = new List<string>();


            Parallel.ForEach(urls, url =>
            {
                Console.WriteLine("Retriving HTML from {0}", url);
                using (var client = new WebClient())
                {
                    var baseHtml = client.DownloadString(url);
                    html.Add(baseHtml);
                }
            });



            return html;
        }

        private List<string> CollectUrls()
        {
            List<string> urls = new List<string>();
            urls.Add(BasePath);

            for (int i = 2; i <= PageCount; i++)
            {
                var url = string.Format("{0}{1}{2}", BasePath, Paging, i);
                urls.Add(url);
            }

            return urls;
        }

        #endregion

    }
}
