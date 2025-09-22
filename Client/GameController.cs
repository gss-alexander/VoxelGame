namespace Client;

public class GameController
{
    private readonly Action _quitAction;
    
    public GameController(Action quitAction)
    {
        _quitAction = quitAction;
    }

    public void QuitGame()
    {
        _quitAction();
    }
}