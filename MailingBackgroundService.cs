using Polly;
using Polly.Retry;
using System;
using System.Linq.Expressions;

namespace WebApplicationMail;

public class MailingBackgroundService : BackgroundService
{
    private readonly ILogger<MailingBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MailingBackgroundService(
        IHostApplicationLifetime applicationLifetime,
        ILogger<MailingBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogInformation("Приложение успешно запущено");
        });
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        while (!stoppingToken.IsCancellationRequested)
        {
            var to = "alexey_teterin89@mail.ru";
            var retryCount = 2;

            AsyncRetryPolicy? policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context)=>
                //.RetryAsync(retryCount, onRetry: (exception, retryAttempt) =>
               {
                   _logger.LogWarning(
                    exception, 
                    "При попытке отправить письмо произошла ошибка, сервис: {Service}, {Recipient}" +
                    "Попытка #{retryAttempt}",
                    mailSender.GetType(),
                    to
                    );
               });

            PolicyResult? result = await policy.ExecuteAndCaptureAsync(
                                       token => mailSender.SendAsync("Родион", to, "Привет", "Все хорошо"), stoppingToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                _logger.LogError(result.FinalException, "Произошла ошибка при попытке отправки письма");
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);




            /*bool sendingSucceeded = false;
            const int attemptsLimit = 10;
            for(var attemptsCount = 1; !sendingSucceeded && attemptsCount <= attemptsLimit && !stoppingToken.IsCancellationRequested; attemptsCount++ )
            {
                try
                {
                    await mailSender.SendAsync("Родион", to, "Привет", "Все хорошо");
                    sendingSucceeded = true;
                }
                catch (Exception e) when (attemptsCount < attemptsLimit)
                {
                    _logger.LogWarning(e,
                    "При попытке отправить письмо произошла ошибка, сервис: {Service}, {Recipient}" +
                    "Попытка #{Attempt}",
                    mailSender.GetType(),
                    to,
                    attemptsCount
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                    "При попытке отправить письмо произошла ошибка, сервис: {Service}, {Recipient}",
                    mailSender.GetType(),
                    to
                    );
                }
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }*/

        }
    }
}