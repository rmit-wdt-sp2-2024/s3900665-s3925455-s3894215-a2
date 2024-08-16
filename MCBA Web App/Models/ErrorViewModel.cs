namespace MCBA_Web_App.Models
{
    // The ErrorViewModel class is used to represent error details in the application.
    // It is typically used to display error information in views.
    public class ErrorViewModel
    {
        // Property to store the unique identifier for the request that caused the error.
        // This is useful for tracking errors and troubleshooting specific issues in logs.
        public string RequestId { get; set; }

        // A boolean property that indicates whether the RequestId should be displayed.
        // It returns true if the RequestId is not null or empty, otherwise returns false.
        // This helps in conditionally showing the RequestId in the error view.
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
