using CookComputing.XmlRpc;

namespace Lykos.Modules
{
    partial class Owner
    {
        partial class Eri
        {
            partial class Update
            {
                [XmlRpcUrl("https://secure.gravatar.com/xmlrpc")]
                public interface IGravatarApi : IXmlRpcProxy
                {
                    [XmlRpcMethod("grav.saveUrl")]
                    string saveUrl(string targetUrl, int rating, string password);

                    [XmlRpcMethod("grav.useUserImage")]
                    bool[] useImage(string targetUserImage, string[] addresses, string password);

                }
            }
        }

    }
}
