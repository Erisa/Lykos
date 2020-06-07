namespace Lykos.Modules
{
    public interface IGravatarApi1
    {
        string SaveUrl(string targetUrl, int rating, string password);
        bool[] UseImage(string targetUserImage, string[] addresses, string password);
    }
}