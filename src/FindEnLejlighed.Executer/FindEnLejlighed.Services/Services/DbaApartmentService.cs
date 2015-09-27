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

namespace FindEnLejlighed.Services.Services
{
    public class DbaApartmentService:IApartmentService
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
            return new Apartment();
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
                    Console.WriteLine("Retriving HTML from {0}",url);
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
