﻿using MediatR;
using Simple_Account_Service.Application.Models;

namespace Simple_Account_Service.Features.Transactions.Commands.TransferBetweenAccounts;

public record TransferBetweenAccountsCommand(Guid AccountId, TransferDto TransferDto,
    Guid CorrelationId) : IRequest<MbResult<List<TransactionDto>>>;