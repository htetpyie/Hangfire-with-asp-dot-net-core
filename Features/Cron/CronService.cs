using System.Diagnostics;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Storage;
using HangfireDotNetCoreExample.Features.DevCodes;
using HangfireDotNetCoreExample.Models;

namespace HangfireDotNetCoreExample.Features.Cron;

public class CronService
{
    public List<CronModel> GetAllCronList()
    {
        List<CronModel> list = new();

        using var connection = JobStorage.Current.GetConnection();
        foreach (var recurringJob in connection.GetRecurringJobs())
        {
            var nextExecutionTime = recurringJob
                .NextExecution
                ?.GetDateTimeString();

            var lastExecutionTime = recurringJob
                .LastExecution
                ?.GetDateTimeString();

            string name = recurringJob.Job.Method.Name;

            var response = new CronModel
            {
                JobId = recurringJob.Id,
                Name = name,
                LastTime = lastExecutionTime ?? "",
                NextTime = nextExecutionTime ?? "",
                IsRunning = true
            };
            list.Add(response);
        }

        return list;
    }
    
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

    public CronModel StopRecurringJob(string jobId)
    {
        CronModel model = new();
        try
        {
            model = GetCronModelByJobId(jobId);
            RecurringJob.RemoveIfExists(jobId); // stop Cron
        }
        catch (Exception e)
        {
            Debug.WriteLine("Exception in stop recurring job " + e.Message);
        }

        return model;
    }

    public List<CronModel> RemoveAllRecurringJob()
    {
        List<CronModel> cronList = new();
        try
        {
            using var connection = JobStorage.Current.GetConnection();
            foreach (var recurringJob in connection.GetRecurringJobs())
            {
                string jobId = recurringJob.Id;
                var cron =  GetCronModelByJobId(jobId);
                cronList.Add(cron);
                
                RecurringJob.RemoveIfExists(jobId);
            }
        }
        catch (Exception ex)
        {
            Debug.Write("Exception in Remove Jobs" + ex.Message);
        }
        return cronList;
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

    public void ContinueJobWith(string jobId,
        [InstantHandle, NotNull] Expression<Action> methodCall)
    {
        BackgroundJob.ContinueJobWith(
            jobId,
            methodCall);
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

    private CronModel GetCronModelByJobId(string jobId)
    {
        var model = new CronModel();
        try
        {
            var recurringJob = GetRecurringJobById(jobId);
            var lastExecutionTime = recurringJob
                .LastExecution
                ?.GetDateTimeString();

            string now = DateTime.Now.GetDateTimeString();

            model = new CronModel
            {
                JobId = jobId,
                Name = recurringJob.Job.Method.Name,
                LastTime = lastExecutionTime ?? "",
                StoppedTime = now,
                IsRunning = false
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return model;
    }

    private RecurringJobDto GetRecurringJobById(string jobId)
    {
        var recurringJobDto = JobStorage
            .Current
            .GetConnection()
            .GetRecurringJobs()
            .FirstOrDefault(x => x.Id.Equals(jobId));
        return recurringJobDto ?? new();
    }
}