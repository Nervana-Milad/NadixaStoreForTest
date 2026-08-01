namespace Nadixa.Core.Interfaces; 
public interface IUserRepository 
{
    Task<int> GetTotalUsersAsync();
    Task<int> GetNewUsersCountAsync(DateTime date);
}