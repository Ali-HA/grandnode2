using Grand.Business.Catalog.Utilities;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//ALI: Gold Service
namespace Grand.Business.Catalog.Interfaces.Prices
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial interface IGoldPriceService
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="weight"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        Task<string[]> s_GoldPrice(decimal weight, decimal ratio);
        

    }
}
