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

        //TODO: AssetUrlHelper: incorporate token later
        public string BuildAssetUrl(string imageUrl)
        {
            string baseUrl = _httpServiceHelper.GetWebServiceUrl();

            string assetUrl = imageUrl.Contains(baseUrl) ? imageUrl : @$"{baseUrl}{Constants.Action_GetAsset}?fileName={imageUrl}";

            return assetUrl;
        }
    }
}
