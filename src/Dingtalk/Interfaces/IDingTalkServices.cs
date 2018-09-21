namespace EaseSource.Dingtalk.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EaseSource.Dingtalk.Entity;

    public interface IDingtalkServices
    {
        Task<string> GetAccessTokenAsync();

        Task<string> GetTicketAsync(string accessToken);

        Task<Dictionary<string, string>> GetPCConfigAsync(string clientUrl);

        Task<DTUser> GetDTUserAsync(string ddUserID);

        Task<string> GetDTUserIDByAuthCodeAsync(string authCode);

        Task<List<DTUser>> GetAllDTUserFullInfoAsync();

        Task<List<DTUser>> GetAllDTUserSimpleInfoAsync();

        Task<List<DTDepartment>> GetDTDepartmentsAsync();

        Task<DTDepartment> GetDTDepartmentInfoAsync(int id);

        Task<List<DTAdmin>> GetDTAdminsAsync();
    }
}