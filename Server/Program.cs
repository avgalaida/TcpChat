using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddNLog("NLog.config");
});