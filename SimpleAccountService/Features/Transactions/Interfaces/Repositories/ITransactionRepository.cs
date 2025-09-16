using JetBrains.Annotations;
using Simple_Account_Service.Application.Interfaces.Repositories;
using Simple_Account_Service.Features.Transactions.Entities;

namespace Simple_Account_Service.Features.Transactions.Interfaces.Repositories;

[UsedImplicitly]
public interface ITransactionRepository : IBaseRepository<Transaction>;