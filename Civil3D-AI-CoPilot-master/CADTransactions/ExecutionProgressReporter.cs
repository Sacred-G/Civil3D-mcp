using System;
using System.Threading;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Represents a single execution step update for live progress reporting.
    /// </summary>
    public class ExecutionStep
    {
        public int StepNumber { get; set; }
        public int TotalSteps { get; set; }
        public string CommandName { get; set; }
        public string Status { get; set; } // "Running", "Success", "Error", "Skipped"
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }

        public ExecutionStep(int stepNumber, int totalSteps, string commandName, string status, string message, string details = null)
        {
            StepNumber = stepNumber;
            TotalSteps = totalSteps;
            CommandName = commandName;
            Status = status;
            Message = message;
            Timestamp = DateTime.Now;
            Details = details;
        }

        public string GetFormattedTime() => Timestamp.ToString("HH:mm:ss.fff");
    }

    /// <summary>
    /// Progress reporter for streaming CAD command execution updates to the UI.
    /// </summary>
    public class ExecutionProgressReporter
    {
        private readonly IProgress<ExecutionStep> _progress;
        private int _currentStep;
        private int _totalSteps;

        public ExecutionProgressReporter(IProgress<ExecutionStep> progress)
        {
            _progress = progress;
        }

        public void Initialize(int totalSteps)
        {
            _totalSteps = totalSteps;
            _currentStep = 0;
        }

        public void ReportStep(string commandName, string status, string message, string details = null)
        {
            _currentStep++;
            var step = new ExecutionStep(_currentStep, _totalSteps, commandName, status, message, details);
            _progress?.Report(step);
        }

        public void ReportRunning(string commandName, string message, string details = null)
        {
            ReportStep(commandName, "Running", message, details);
        }

        public void ReportSuccess(string commandName, string message, string details = null)
        {
            ReportStep(commandName, "Success", message, details);
        }

        public void ReportError(string commandName, string message, string details = null)
        {
            ReportStep(commandName, "Error", message, details);
        }

        public void ReportSkipped(string commandName, string message, string details = null)
        {
            ReportStep(commandName, "Skipped", message, details);
        }
    }
}
