namespace OperatingSystemSimulator.Apps.WebBrowser.Pages;
public sealed partial class ErrorPage : UserControl
{
    public ErrorPage(ErrorCodes message, string address)
    {
        InitializeComponent();

        if (message == ErrorCodes.ERR_NAME_NOT_RESOLVED)
        {
            ErrorTextBlock.Text = $"Error: {message.GetDescription()}\nAdress: {address}\n\nError Code: {message}";
        }
        else
        {
            ErrorTextBlock.Text = $"Error: {message.GetDescription()}\n\nError Code: {message}";
        }
    }
}
