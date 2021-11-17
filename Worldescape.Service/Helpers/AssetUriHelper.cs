using System;
using System.Collections.Generic;
using System.Text;

namespace Worldescape.Service
{
    public class AssetUriHelper
    {
        //TODO: incorporate token later
        public string BuildAssetUri(string imageUrl)
        {
#if DEBUG
            string assetUri = @$"{Properties.Resources.DevWebService}/api/Query/GetAsset?fileName={imageUrl}";
#else
            string assetUri = @$"{Properties.Resources.ProdWebService}/api/Query/GetAsset?fileName={assetName}";
#endif

            return assetUri;

        }
    }
}
