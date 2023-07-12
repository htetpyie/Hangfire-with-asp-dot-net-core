using System.Diagnostics;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Storage;
using HangfireDotNetCoreExample.Models;

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

    public void ContinueJobWith(string jobId, 
        [InstantHandle, NotNull] Expression<Action> methodCall)
    {
        BackgroundJob.ContinueJobWith(
            jobId,
            methodCall);
    }

    public List<CronModel> GetAllCronList()
    {
        List<CronModel> list = new();

        using var connection = JobStorage.Current.GetConnection();
        foreach (var recurringJob in connection.GetRecurringJobs())
        {
            var nextExecutionTime = TimeZoneInfo
                .ConvertTimeBySystemTimeZoneId(
                    recurringJob.NextExecution ?? DateTime.Now,
                    "Myanmar Standard Time")
                .ToString("f");
            
            var lastExecutionTime =
                (recurringJob.LastExecution == null)
                    ? ""
                    : TimeZoneInfo
                        .ConvertTimeBySystemTimeZoneId(
                            recurringJob.LastExecution ?? DateTime.Now,
                            "Myanmar Standard Time")
                        .ToString("f");

            string name = recurringJob.Job.Method.Name;

            var response = new CronModel
            {
                JobId = recurringJob.Id,
                Name = name,
                LastTime = lastExecutionTime,
                NextTime = nextExecutionTime ?? "",
                IsRunning = true
            };
            list.Add(response);
        }

        return list;
    }
}