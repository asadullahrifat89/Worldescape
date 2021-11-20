namespace Worldescape.Service
{
    public class AssetUrlHelper
    {
        //TODO: AssetUrlHelper: incorporate token later
        public string BuildAssetUrl(string imageUrl)
        {
#if DEBUG
            string baseUrl = Properties.Resources.DevWebService;
#else
            string baseUrl = Properties.Resources.ProdWebService;            
#endif
            string assetUrl = imageUrl.Contains(baseUrl) ? imageUrl : @$"{baseUrl}/api/Query/GetAsset?fileName={imageUrl}";

            return assetUrl;
        }
    }
}
