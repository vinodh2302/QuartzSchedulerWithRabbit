using Quartz;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // Register RabbitMQ publisher service
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

        // Register all your CronJobs
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();

            // Orders Cron Job
            //var ordersJobKey = new JobKey("OrdersCronJob");
            //q.AddJob<OrdersCronJob>(opts => opts.WithIdentity(ordersJobKey));
            //q.AddTrigger(opts => opts
            //    .ForJob(ordersJobKey)
            //    .WithIdentity("OrdersCronJob-trigger")
            //    .WithCronSchedule("0 0/1 * * * ?")); // every 1 min

            // Customers Cron Job
            var customersJobKey = new JobKey("CustomersCronJob");
            q.AddJob<CustomersCronJob>(opts => opts.WithIdentity(customersJobKey));
            q.AddTrigger(opts => opts
                .ForJob(customersJobKey)
                .WithIdentity("CustomersCronJob-trigger")
                .WithCronSchedule("0/15 * * * * ?")); // every 2 min
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    })
    .Build();

await host.RunAsync();
