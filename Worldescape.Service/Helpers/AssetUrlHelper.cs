using Worldescape.Data;

namespace Worldescape.Service
{
    public class AssetUrlHelper
    {
        private readonly HttpServiceHelper _httpServiceHelper;

        public AssetUrlHelper(HttpServiceHelper httpCommunicationService)
        {
            _httpServiceHelper = httpCommunicationService;
        }

        public string BuildAssetUrl(string token, string imageUrl)
        {
            string baseUrl = _httpServiceHelper.GetWebServiceUrl();

            string assetUrl = imageUrl.Contains(baseUrl) ? imageUrl : @$"{baseUrl}{Constants.Action_GetAsset}?token={token}&fileName={imageUrl}";

            return assetUrl;
        }
    }
}
