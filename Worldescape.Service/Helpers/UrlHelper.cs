using Worldescape.Common;

namespace Worldescape.Service
{
    public class UrlHelper
    {
        private readonly HttpServiceHelper _httpServiceHelper;

        public UrlHelper(HttpServiceHelper httpCommunicationService)
        {
            _httpServiceHelper = httpCommunicationService;
        }

        public string BuildAssetUrl(string token, string imageUrl)
        {
            string baseUrl = _httpServiceHelper.GetWebServiceUrl();

            string assetUrl = imageUrl.Contains(baseUrl) ? imageUrl : @$"{baseUrl}{Constants.Action_GetAsset}?token={token}&fileName={imageUrl}";

            return assetUrl;
        }

        public string BuildBlobUrl(string token, string id)
        {
            string baseUrl = _httpServiceHelper.GetWebServiceUrl();

            string assetUrl = @$"{baseUrl}{Constants.Action_GetBlob}?token={token}&id={id}";

            return assetUrl;
        }
    }
}
