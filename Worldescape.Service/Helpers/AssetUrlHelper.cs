using Worldescape.Data;

namespace Worldescape.Service
{
    public class AssetUrlHelper
    {
        private readonly HttpCommunicationService _httpCommunicationService;

        public AssetUrlHelper(HttpCommunicationService httpCommunicationService) 
        {
            _httpCommunicationService = httpCommunicationService;        
        }

        //TODO: AssetUrlHelper: incorporate token later
        public string BuildAssetUrl(string imageUrl)
        {
            string baseUrl = _httpCommunicationService.GetWebServiceUrl();

            string assetUrl = imageUrl.Contains(baseUrl) ? imageUrl : @$"{baseUrl}{Constants.Action_GetAsset}?fileName={imageUrl}";

            return assetUrl;
        }
    }
}
