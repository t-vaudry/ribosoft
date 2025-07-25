/*
 * Simple diagnostic script to check and restart stuck jobs
 * 
 * Usage:
 * 1. Replace YOUR_JOB_ID with the actual job ID
 * 2. Run: dotnet run --project Ribosoft
 * 3. In another terminal, use this as reference to manually execute the logic
 */

using System;
using Microsoft.EntityFrameworkCore;
using Ribosoft.Data;
using Ribosoft.Models;
using Hangfire;

// This is a reference implementation - you'll need to adapt it to your specific job ID

public class JobDiagnostic
{
    public static void DiagnoseJob(int jobId)
    {
        // You would need to get the connection string from appsettings.json
        var connectionString = "Host=192.168.2.50;Port=5432;Database=ribosoft;Username=postgres;Password=AWtSkw6iuVBjK9v;Pooling=true;";
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;
            
        using var context = new ApplicationDbContext(options);
        
        var job = context.Jobs.FirstOrDefault(j => j.Id == jobId);
        
        if (job == null)
        {
            Console.WriteLine($"Job {jobId} not found!");
            return;
        }
        
        Console.WriteLine($"=== JOB DIAGNOSIS for Job {jobId} ===");
        Console.WriteLine($"Current JobState: {job.JobState} (value: {(int)job.JobState})");
        Console.WriteLine($"TargetEnvironment: {job.TargetEnvironment}");
        Console.WriteLine($"StatusMessage: '{job.StatusMessage}'");
        Console.WriteLine($"CreatedAt: {job.CreatedAt}");
        Console.WriteLine($"UpdatedAt: {job.UpdatedAt}");
        
        // Check conditions
        bool phase2Condition = job.JobState == JobState.Structure && job.TargetEnvironment == TargetEnvironment.InVivo;
        bool phase3Condition = job.JobState == JobState.Structure && job.TargetEnvironment == TargetEnvironment.InVitro;
        
        Console.WriteLine($"Phase2 condition (Structure + InVivo): {phase2Condition}");
        Console.WriteLine($"Phase3 condition (Structure + InVitro): {phase3Condition}");
        
        if (phase2Condition)
        {
            Console.WriteLine("Job SHOULD transition to Phase2 (BLAST analysis)");
            Console.WriteLine("To restart: BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase2(jobId, JobCancellationToken.Null));");
        }
        else if (phase3Condition)
        {
            Console.WriteLine("Job SHOULD transition to Phase3 (Multi-objective optimization)");
            Console.WriteLine("To restart: BackgroundJob.Enqueue<GenerateCandidates>(x => x.Phase3(jobId, JobCancellationToken.Null));");
        }
        else
        {
            Console.WriteLine("Job does NOT meet conditions for Phase2 or Phase3!");
        }
    }
}
