using System.Net;

namespace Simple_Account_Service.Application.Models;

public record MbError(HttpStatusCode Code, string Message = "", string Description = "");