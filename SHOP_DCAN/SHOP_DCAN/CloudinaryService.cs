using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace SHOP_DCAN
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService()
        {
            var account = new Account(
                ConfigurationManager.AppSettings["Cloudinary.CloudName"],
                ConfigurationManager.AppSettings["Cloudinary.ApiKey"],
                ConfigurationManager.AppSettings["Cloudinary.ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public string GetImageUrl(string imageName)
        {
            return _cloudinary.Api.UrlImgUp.BuildUrl(imageName);
        }
    }
}