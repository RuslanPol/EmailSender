using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Massege;

public class ServerStartupNotificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private readonly ILogger<ServerStartupNotificationBackgroundService> _logger;

    public int RetryCount { get; set; }


    public ServerStartupNotificationBackgroundService(IServiceScopeFactory serviceScopeFactory,
        ILogger<ServerStartupNotificationBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
         await using (var scope = _serviceScopeFactory.CreateAsyncScope())
        {
           
            var _emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

           
            var email = "rpolisuk72gmail.com";


            int retryCount = 3;
            AsyncRetryPolicy policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                     (exception, retryAttempt) =>
                    {
                         _logger.LogWarning(exception, "Error while sending email. Retrying: {Attempt}", retryAttempt);
                    });


            PolicyResult result =await policy.ExecuteAndCaptureAsync( //ЗапуститьИЗафиксировать()
                token =>
                      _emailSender.Send(email, $"Сервер запущен", "Сервер удачно запущен"), stoppingToken);

            if ( result.Outcome == OutcomeType.Failure)
            {
                _logger.LogError(result.FinalException, "There was an error while sending email");
            }
        }
    }
}

// .Retry(retryCount, onRetry: (exception, retryAttempt) =>
// {
//     _logger.LogWarning(
//         exception, "Error while sending email. Retrying: {Attempt}", retryAttempt);
//
//
// })

// {
//     _logger.LogWarning(
//          "Error while sending email. Retrying: {Attempt}"
//     );
// }
// _logger.LogWarning(
//             exception, "Error while sending email. Retrying: {Attempt}"
//
//         );