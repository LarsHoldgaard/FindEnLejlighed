using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using FindEnLejlighed.Services.Domain;
using HtmlAgilityPack;
using Newtonsoft.Json;
using static System.Decimal;

namespace FindEnLejlighed.Services.Services
{
    public class DbaApartmentService : IApartmentService
    {
        private const string BasePath = "http://www.dba.dk/boliger/lejebolig/reg-koebenhavn/";
        private const string Paging = "side-";
        public List<Apartment> GetApartments()
        {
            List<Apartment> apartments = new List<Apartment>();
            var html = GetBaseHtml(CollectUrls());

            foreach (var singleHtml in html)
            {
                var links = GetApartmentLinksOnPage(singleHtml);

                foreach (var apartmentLink in links)
                {
                    var apartment = ParseApartment(apartmentLink);
                    apartments.Add(apartment);
                }
            }

            return apartments;
        }

        #region Get single apartment link

        private Apartment ParseApartment(string url)
        {
            string html = string.Empty;
            Console.WriteLine("Pulling info from {0}", url);

            using (var client = new WebClient())
            {
                html = client.DownloadString(url);

            }
            var apartment = new Apartment()
            {
                ContactStatus = ContactStatus.New,
                Link = url
            };

            FillApartment(apartment, html, url);

            return apartment;

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

                apartment.CityRegion = layer.a.attr.CityRegion;
                apartment.TakeOverDate = layer.a.attr.TakeoverDate;
                apartment.SellerType = layer.a.attr.SELLER_TYPE;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Datalayer didn't work on ", apartment.Link);
                throw ex;
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

            using (var client = new WebClient())
            {
                foreach (var url in urls)
                {
                    Console.WriteLine("Retriving HTML from {0}", url);
                    var baseHtml = client.DownloadString(BasePath);
                    html.Add(baseHtml);
                }

            }

            return html;
        }

        private List<string> CollectUrls()
        {
            List<string> urls = new List<string>();
            urls.Add(BasePath);

            for (int i = 2; i <= 2; i++)
            {
                var url = string.Format("{0}{1}{2}", BasePath, Paging, i);
                urls.Add(url);
            }

            return urls;
        }

        #endregion

    }
}
