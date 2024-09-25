namespace GeneralGame.HUD;
using GeneralGame;

[StyleSheet]
public partial class Confirm : Panel
{
    public static Confirm Instance { get; private set; }
   
    private string dialogMessage = "";
    private TaskCompletionSource<bool> taskCompletionSource;
    public ItemComponent SelectedItem { get; private set; }

    public Confirm() => Instance = this;

    public Task<bool> ShowConfirmationDialog( ItemComponent item, string message )
    {
        SelectedItem = item;
        dialogMessage = message;
     
        StateHasChanged();
        taskCompletionSource = new TaskCompletionSource<bool>();
        FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.Confirm );
        return taskCompletionSource.Task;
    }

    public void HideDialog()
    {
        
        dialogMessage = ""; // Nachricht zurücksetzen
        StateHasChanged();
        FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
    }

    private void ConfirmAction()
    {
        if ( taskCompletionSource == null )
        {
            Log.Error( "taskCompletionSource is null in ConfirmAction" );
            return; // Beenden Sie die Methode, wenn taskCompletionSource null ist
        }

        Log.Info( "ConfirmAction" );
   
        dialogMessage = ""; // Nachricht zurücksetzen
        StateHasChanged();
        taskCompletionSource.SetResult( true );
        FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
    }

    private void CancelAction()
    {
        if ( taskCompletionSource == null )
        {
            Log.Error( "taskCompletionSource is null in CancelAction" );
            return; // Beenden Sie die Methode, wenn taskCompletionSource null ist
        }


        dialogMessage = ""; // Nachricht zurücksetzen
        StateHasChanged();
        taskCompletionSource.SetResult( false );
        FullScreenManager.Instance.Display( FullScreenManager.FullScreenPanel.InGameHud );
    }
}