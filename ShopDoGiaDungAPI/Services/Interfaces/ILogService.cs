namespace ShopDoGiaDungAPI.Services.Interfaces
{
    public interface ILogService
    {
        void WriteLog(string userid, string action, string objects, string ip);
        Task WriteLogAsync(string userid, string action, string objects, string ip);
    }
}
