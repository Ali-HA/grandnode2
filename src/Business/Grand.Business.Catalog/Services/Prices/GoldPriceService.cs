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
    public enum GoldPriceStatus { 
        UseCurrentValue,
        WaitingForResponse,
        ResponseError,
        RequestNewValue
    }

    /// <summary>
    /// Pricing service
    /// </summary>
    public partial class GoldPriceService : IGoldPriceService
    {
        #region Fields

        static HttpClient client = new HttpClient();
        public static DateTime? retrieveTime { get; set; }
        public static decimal gprice_last { get; set; }
        public static GoldPriceStatus GPStatus { get; set; } = GoldPriceStatus.RequestNewValue;
        public static int CallCount { get; set; } = 0;
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

        public class dtoGoldPrice
        {
            public int timestamp { get; set; }
            public string metal { get; set; }
            public string currency { get; set; }
            public string exchange { get; set; }
            public string symbol { get; set; }
            public int open_time { get; set; }
            public double price { get; set; }
            public double ch { get; set; }
            public double ask { get; set; }
            public double bid { get; set; }
        }

        #endregion

        #region Utilities




        #endregion

        #region Methods

        public virtual async Task<string[]> s_GoldPrice(decimal weight, decimal ratio)
        {
            System.Diagnostics.Debug.WriteLine("GPrice service call:{0}",++CallCount);
            //string[] gp_res;
            decimal price = 0.0m;

            switch (GPStatus)
            {
                case GoldPriceStatus.UseCurrentValue:
                    System.Diagnostics.Debug.WriteLine("Use Current");
                    if (retrieveTime.HasValue && ((DateTime.Now - retrieveTime.Value).TotalSeconds > 5400)) //retirve new value every 90min ==> 4500s
                    {
                        System.Diagnostics.Debug.WriteLine("Use Current -> request New Value");
                        GPStatus = GoldPriceStatus.RequestNewValue;
                        await s_GoldPrice(weight, ratio);    //make new request after passing of 10mins
                    }
                    break;
                case GoldPriceStatus.WaitingForResponse:
                    System.Diagnostics.Debug.WriteLine("wait For Response");
                    break;
                case GoldPriceStatus.ResponseError:
                    System.Diagnostics.Debug.WriteLine("RequestError");
                    if ((DateTime.Now - retrieveTime.Value).TotalSeconds > 600)
                    {
                        System.Diagnostics.Debug.WriteLine("RequestError > 600");
                        GPStatus = GoldPriceStatus.RequestNewValue;
                        await s_GoldPrice(weight,ratio);    //make new request after passing of 10mins
                    }
                    break;
                case GoldPriceStatus.RequestNewValue:
                    System.Diagnostics.Debug.WriteLine("RequestNew");
                    try
                    {
                        const string url = @"https://www.goldapi.io/api/XAU/KWD";
                        var x = client.DefaultRequestHeaders.UserAgent;
                        var h = client.DefaultRequestHeaders;
                        ;
                        if (!h.Contains("x-access-token"))
                        {
                            client.DefaultRequestHeaders.Add("x-access-token", "goldapi-11yh2ayke5xtmr3-io");
                        }
                        GPStatus = GoldPriceStatus.WaitingForResponse;
                        HttpResponseMessage response = await client.GetAsync(url);
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            System.Diagnostics.Debug.WriteLine("RequestNew - > error");
                            GPStatus = GoldPriceStatus.ResponseError;
                            retrieveTime = DateTime.Now;
                            break;
                        }
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions {
                            AllowTrailingCommas = true
                        };
                        dtoGoldPrice gp = JsonSerializer.Deserialize<dtoGoldPrice>(responseBody, options);
                        price = (decimal)(gp.price / 28.34952);
                        gprice_last = price;
                        price = price * weight * ratio;
                        retrieveTime = DateTime.Now;
                        System.Diagnostics.Debug.WriteLine("RequestNew -> use current");
                        GPStatus = GoldPriceStatus.UseCurrentValue;
                    }
                    catch (Exception err)
                    {
                        retrieveTime = DateTime.Now;
                        GPStatus = GoldPriceStatus.ResponseError;
                        System.Diagnostics.Debug.WriteLine("GP service call Exception:{0}\r\n{1}", err.Message, err?.InnerException.Message);
                    }
                    break;
                default:
                    break;
            }

            return new string[] { (gprice_last * weight * ratio).ToString("F3"), retrieveTime.HasValue? retrieveTime.Value.ToString(): DateTime.Now.ToString() };

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
