using Grand.Business.Catalog.Extensions;
using Grand.Business.Catalog.Interfaces.Categories;
using Grand.Business.Catalog.Interfaces.Discounts;
using Grand.Business.Catalog.Interfaces.Collections;
using Grand.Business.Catalog.Interfaces.Prices;
using Grand.Business.Catalog.Interfaces.Products;
using Grand.Business.Catalog.Queries.Models;
using Grand.Business.Catalog.Utilities;
using Grand.Business.Common.Interfaces.Directory;
using Grand.Infrastructure;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Orders;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Business.Catalog.Interfaces.Brands;
using System.Net.Http;
using System.Text.Json;
//ALI: Gold Service
namespace Grand.Business.Catalog.Services.Prices
{
    /// <summary>
    /// Pricing service
    /// </summary>
    public partial class GoldPriceService : IGoldPriceService
    {
        #region Fields

        static HttpClient client = new HttpClient();
        public static DateTime? retrieveTime { get; set; }
        public static decimal gprice_last { get; set; }

        #endregion

        #region Ctor

        public GoldPriceService()
        {
            //retrieveTime = DateTime.Now;
        }

        #endregion

        #region Nested classes

        public class js_GPriceContainer
        {
            public int draw { get; set; }
            public js_GoldPrice[] data { get; set; }
            public string update_time { get; set; }

        }
        public class js_GoldPrice
        {
            public string gold_type { get; set; }
            public string price_dinar { get; set; }
            public string price_usd { get; set; }

        }
        #endregion

        #region Utilities

       


        #endregion

        #region Methods

        public virtual async Task<string[]> s_GoldPrice(decimal weight, decimal ratio)
        {
            string[] gp_res;
            decimal price = 0.0m;
            //if (retrieveTime.HasValue && ((DateTime.Now - retrieveTime.Value).TotalSeconds < 5))

            //check if we already retirved the price today, if so then return the stored price value
            if (retrieveTime.HasValue && (DateTime.Now.Date ==  retrieveTime.Value.Date) )
            {
                price = gprice_last * weight * ratio;
                gp_res = new string[] { price.ToString("F2"), retrieveTime.Value.ToString() };
                return gp_res;
            }

            //otherwise, fetch the latest price from MOCI site
            try
            {

                //const string url = @"https://new2.moci.gov.kw/ar/gold/table_api/?action_name=ajax_table&draw=103&columns[0][data]=gold_type&columns[0][name]=&columns[0][searchable]=true&columns[0][orderable]=false&columns[0][search][value]=&columns[0][search][regex]=false&columns[1][data]=price_dinar&columns[1][name]=&columns[1][searchable]=true&columns[1][orderable]=false&columns[1][search][value]=&columns[1][search][regex]=false&columns[2][data]=price_usd&columns[2][name]=&columns[2][searchable]=true&columns[2][orderable]=false&columns[2][search][value]=&columns[2][search][regex]=false&start=0&length=-1&search[value]=&search[regex]=false&_=1598317869725";
                const string url = @"https://moci.gov.kw/en/market-prices/gold/";   //updated url. paged to be parsed and price retrieved

                var x = client.DefaultRequestHeaders.UserAgent;
                if (x.Count == 0)
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                }

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //var options = new JsonSerializerOptions {
                //    AllowTrailingCommas = true
                //};

                //js_GPriceContainer x = JsonSerializer.Deserialize<js_GPriceContainer>(responseBody, options);

                //decimal.TryParse(x?.data[0]?.price_dinar, out price);
                price = ParseForPrice(responseBody);
                gprice_last = price;
                price = price * weight * ratio;
                //gp_res = new string[] { price.ToString("F2"), x?.update_time };
                gp_res = new string[] { price.ToString("F2"), DateTime.Now.ToString() };
                retrieveTime = DateTime.Now;
                return gp_res;

            }
            catch (HttpRequestException e)
            {

                if (retrieveTime.HasValue)
                {
                    price = gprice_last * weight * ratio;
                    gp_res = new string[] { price.ToString("F2"), retrieveTime.Value.ToString() };
                    return gp_res;
                }

            }
            return new string[] { gprice_last.ToString(), retrieveTime.Value.ToString() };
        }


        static decimal ParseForPrice(string html)
        {
            var start = html.IndexOf("price_dinar") + "price_dinar".Length + 2;
            var end = html.IndexOf("</p>", start);
            var goldPrice = html.Substring(start, end - start);
            var gp = goldPrice.Replace("KWD", "").Trim();
            decimal price;
            decimal.TryParse(gp, out price);
            return price;
        }
        #endregion
    }
}
