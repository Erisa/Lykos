namespace Lykos.Modules
{
    public interface IGravatarApi1
    {
        string saveUrl(string targetUrl, int rating, string password);
        bool[] useImage(string targetUserImage, string[] addresses, string password);
    }
}