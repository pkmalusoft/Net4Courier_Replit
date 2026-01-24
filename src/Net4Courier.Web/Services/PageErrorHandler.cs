using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Net4Courier.Web.Services;

public interface IPageErrorHandler
{
    void HandleError(Exception ex, string? customMessage = null);
    void ShowWarning(string message);
    void ShowInfo(string message);
    void ShowSuccess(string message);
    Task<bool> TryExecuteAsync(Func<Task> action, string? errorMessage = null);
    Task<T?> TryExecuteAsync<T>(Func<Task<T>> action, string? errorMessage = null);
}

public class PageErrorHandler : IPageErrorHandler
{
    private readonly ISnackbar _snackbar;
    private readonly ILogger<PageErrorHandler> _logger;

    public PageErrorHandler(ISnackbar snackbar, ILogger<PageErrorHandler> logger)
    {
        _snackbar = snackbar;
        _logger = logger;
    }

    public void HandleError(Exception ex, string? customMessage = null)
    {
        _logger.LogError(ex, "Page error: {Message}", ex.Message);
        
        var message = customMessage ?? GetUserFriendlyMessage(ex);
        _snackbar.Add(message, Severity.Error, config =>
        {
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = 10000;
        });
    }

    public void ShowWarning(string message)
    {
        _snackbar.Add(message, Severity.Warning, config =>
        {
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = 5000;
        });
    }

    public void ShowInfo(string message)
    {
        _snackbar.Add(message, Severity.Info, config =>
        {
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = 5000;
        });
    }

    public void ShowSuccess(string message)
    {
        _snackbar.Add(message, Severity.Success, config =>
        {
            config.ShowCloseIcon = true;
            config.VisibleStateDuration = 3000;
        });
    }

    public async Task<bool> TryExecuteAsync(Func<Task> action, string? errorMessage = null)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex)
        {
            HandleError(ex, errorMessage);
            return false;
        }
    }

    public async Task<T?> TryExecuteAsync<T>(Func<Task<T>> action, string? errorMessage = null)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            HandleError(ex, errorMessage);
            return default;
        }
    }

    private static string GetUserFriendlyMessage(Exception ex)
    {
        var message = ex.Message.ToLowerInvariant();
        
        if (message.Contains("42703") || message.Contains("column") && message.Contains("does not exist"))
            return "Database configuration issue. A required column is missing. Please contact support.";
        
        if (message.Contains("42p01") || message.Contains("relation") && message.Contains("does not exist"))
            return "Database table is missing. Please ensure the database is properly set up.";
        
        if (message.Contains("23505") || message.Contains("duplicate key"))
            return "This record already exists. Please use a unique value.";
        
        if (message.Contains("23503") || message.Contains("foreign key"))
            return "Cannot complete this action because it references other data.";
        
        if (message.Contains("connection") || message.Contains("timeout"))
            return "Unable to connect to the database. Please try again.";
        
        if (message.Contains("unauthorized") || message.Contains("permission"))
            return "You don't have permission to perform this action.";
        
        if (ex is InvalidOperationException && message.Contains("sequence"))
            return "Error loading data. Please refresh the page.";

        if (ex is ArgumentNullException or ArgumentException)
            return "Invalid input provided. Please check your data.";

        if (ex is Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            return "This record was modified by another user. Please refresh and try again.";

        if (ex is Microsoft.EntityFrameworkCore.DbUpdateException)
            return "Error saving data. Please check your input and try again.";

        return "An unexpected error occurred. Please try again or contact support.";
    }
}
