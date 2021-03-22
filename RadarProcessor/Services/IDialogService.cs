namespace RadarProcessor.Services
{
    public interface IDialogService
    {
        string BrowseFile(string filter);

        void ShowError(string errorMessage);

        bool ShowMessage(string message);
    }
}