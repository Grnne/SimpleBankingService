using Simple_Account_Service.Application.Interfaces;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Transactions.Interfaces.Repositories;

public interface ITransactionRepository : IBaseRepository<Transaction>
{

}