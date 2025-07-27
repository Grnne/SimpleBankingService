using Simple_Account_Service.Features.Accounts.Commands.CreateAccount;
using Simple_Account_Service.Features.Accounts.Commands.UpdateAccount;
using Simple_Account_Service.Features.Accounts.Queries.GetAccountStatement.Dto;

namespace Simple_Account_Service.Features.Accounts.Interfaces;

public interface IAccountsService
{
    Task<AccountDto> CreateAccountAsync(CreateAccountDto createAccountDto);

    Task<AccountDto> UpdateAccountAsync(Guid accountId, UpdateAccountDto request);

    Task DeleteAccountAsync(Guid accountId);

    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();

    Task<MultiAccountStatementDto> GetAccountStatement(Guid ownerId, Guid? accountId, DateTime startDate, DateTime endDate);

    Task<bool> CheckAccountExists(Guid accountId);
}