namespace CoreAccess.WebAPI.Services.InitialSetup;

public class SetupStatusService
{
    private bool _setupCompleted;
    private bool _setupInProgress;
    private int _setupPercentage;
    private List<string> _setupSteps = new();
    private int _currentStepIndex = 0;
    private readonly object _lock = new();

    public bool IsSetupRequired => !_setupCompleted;
    public bool IsSetupInProgress => _setupInProgress;

    public void StartSetup()
    {
        lock (_lock)
        {
            _setupSteps.AddRange([
                "Create Admin User",
                "Configure JWT Settings",
                "Finalize Setup"
            ]);
            _setupInProgress = true;
        }
    }

    public void UpdateSetupProgress(string stepName, int percentage)
    {
        lock (_lock)
        {
            if (percentage < 0 || percentage > 99)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 99.");

            _setupPercentage = percentage;

            if (!_setupSteps.Contains(stepName) || string.IsNullOrWhiteSpace(stepName))
                throw new ArgumentException($"Invalid step name: {stepName}");

            _currentStepIndex = _setupSteps.IndexOf(stepName);
        }
    }

    public void CompleteSetup()
    {
        lock (_lock)
        {
            _setupPercentage = 100;
            _setupCompleted = true;
            _setupInProgress = false;
        }
    }
}