﻿using Grand.Business.Catalog.Interfaces.Prices;
using Grand.Business.Common.Interfaces.Directory;
using Microsoft.AspNetCore.Mvc;
using Shipping.ShippingPoint.Models;
using Shipping.ShippingPoint.Services;
using System.Threading.Tasks;

namespace Shipping.ShippingPoint.Controllers
{
    public class SelectedShippingPointController : Controller
    {
        private readonly IShippingPointService _shippingPointService;
        private readonly ICountryService _countryService;
        private readonly IPriceFormatter _priceFormatter;

        public SelectedShippingPointController(IShippingPointService shippingPointService, ICountryService countryService, IPriceFormatter priceFormatter)
        {
            _shippingPointService = shippingPointService;
            _countryService = countryService;
            _priceFormatter = priceFormatter;
        }
        public async Task<IActionResult> Get(string shippingOptionId)
        {
            var shippingPoint = await _shippingPointService.GetStoreShippingPointById(shippingOptionId);
            if (shippingPoint != null)
            {
                var viewModel = new PointModel() {
                    ShippingPointName = shippingPoint.ShippingPointName,
                    Description = shippingPoint.Description,
                    PickupFee = _priceFormatter.FormatShippingPrice(shippingPoint.PickupFee),
                    OpeningHours = shippingPoint.OpeningHours,
                    Address1 = shippingPoint.Address1,
                    City = shippingPoint.City,
                    CountryName = (await _countryService.GetCountryById(shippingPoint.CountryId))?.Name,
                    ZipPostalCode = shippingPoint.ZipPostalCode,
                };
                return View(viewModel);
            }
            return Content("ShippingPointController: given Shipping Option doesn't exist");
        }
    }
}
