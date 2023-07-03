using System.Diagnostics;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Storage;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronService
{
    public void CreateRecurringJob(string jobId,
        [NotNull, InstantHandle] Expression<Action> methodCall,
        string cron)
    {
        try
        {
            // var manager = new RecurringJobManager();
            // manager.AddOrUpdate(
            //     jobId,
            //     methodCall,
            //     cron);
            // manager.TriggerJob(jobId);
            RecurringJobOptions options = new RecurringJobOptions()
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Myanmar Standard Time")
            };
            RecurringJob.AddOrUpdate(jobId, methodCall, cron, options);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public string CreateBackgroundJob(
        [NotNull, InstantHandle] Expression<Action> methodCall,
        TimeSpan delay)
    {
        string jobId = "";
        try
        {
            jobId = BackgroundJob.Schedule(
                methodCall,
                delay);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return jobId;
    }

    public void StopRecurringJob(string jobId)
    {
        try
        {
            RecurringJob.RemoveIfExists(jobId);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception in stop recurring job " + e.Message);
        }
    }

    public void DeleteBackgroundJob(string jobId)
    {
        try
        {
            BackgroundJob.Delete(jobId);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    public void RemoveAllRecurringJob()
    {
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                RecurringJob.RemoveIfExists(recurringJob.Id);
            }
        }
        catch (Exception ex)
        {
            Debug.Write("Exception in Remove Jobs" + ex.Message);
        }
    }
}