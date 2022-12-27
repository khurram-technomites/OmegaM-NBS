using NowBuySell.Data;
using System.Collections.Generic;

namespace NowBuySell.Web.Areas.Admin.ViewModels.Banners
{
	public class BannersViewModel
	{
		public List<BannerImage> WebsiteBanners { get; set; }
		public List<BannerImage> MobileBanners { get; set; }
		public List<BannerImage> PromotionBanners { get; set; }

        public List<PromoBanner> PromoBannerWeb { get; set; }
        public List<PromoBanner> PromoBannerMobile { get; set; }
    }
}