﻿using Grand.Business.Catalog.Interfaces.Products;
using Grand.Business.Storage.Interfaces;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Api.Commands.Models.Catalog
{
    public class UpdateProductPictureCommandHandler : IRequestHandler<UpdateProductPictureCommand, bool>
    {
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;

        public UpdateProductPictureCommandHandler(
            IProductService productService,
            IPictureService pictureService)
        {
            _productService = productService;
            _pictureService = pictureService;
        }

        public async Task<bool> Handle(UpdateProductPictureCommand request, CancellationToken cancellationToken)
        {
            var product = await _productService.GetProductById(request.Product.Id, true);

            var productPicture = product.ProductPictures.Where(x => x.PictureId == request.Model.PictureId).FirstOrDefault();
            if (productPicture == null)
                throw new ArgumentException("No product picture found with the specified id");

            var picture = await _pictureService.GetPictureById(productPicture.PictureId);
            if (picture == null)
                throw new ArgumentException("No picture found with the specified id");

            productPicture.DisplayOrder = request.Model.DisplayOrder;            
            await _productService.UpdateProductPicture(productPicture, product.Id);

            await _pictureService.UpdatePicture(picture.Id,
                await _pictureService.LoadPictureBinary(picture),
                picture.MimeType,
                request.Model.SeoFilename,
                request.Model.AltAttribute,
                request.Model.TitleAttribute);

            return true;
        }
    }
}
