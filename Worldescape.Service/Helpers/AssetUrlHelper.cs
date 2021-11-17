using System;
using System.Collections.Generic;
using System.Text;

namespace Worldescape.Service
{
    public class AssetUrlHelper
    {
        //TODO: incorporate token later
        public string BuildAssetUrl(string imageUrl)
        {

#if DEBUG
            string baseUrl = Properties.Resources.DevWebService;
            string assetUrl = imageUrl.Contains(baseUrl) ? imageUrl : @$"{Properties.Resources.DevWebService}/api/Query/GetAsset?fileName={imageUrl}";
#else
            string baseUrl = Properties.Resources.ProdWebService;
            string assetUri = imageUrl.Contains(baseUrl) ? imageUrl : @$"{Properties.Resources.ProdWebService}/api/Query/GetAsset?fileName={assetName}";
#endif

            return assetUrl;

        }
    }
}
