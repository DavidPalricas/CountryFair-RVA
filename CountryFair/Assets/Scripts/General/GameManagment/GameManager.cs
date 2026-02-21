public class GameManager
{   
    private static GameManager instance;

    public bool IntroCompleted { get; set; } = false;
    public bool FrisbeeTutorialCompleted { get; set; } = true;

    public bool ArcheryTutorialCompleted { get; set; } = false;

    public bool FrisbeeSessionCompleted { get; set; } = false;
    public bool ArcherySessionCompleted { get; set; } = false;

    private GameManager() { }

    public static GameManager GetInstance()
    {
        instance ??= new GameManager();

        return instance;
    }
}