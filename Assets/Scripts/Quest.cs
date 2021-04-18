public class Quest
{
    private readonly string questName;
    private bool isCompleted;
    
    public Quest(string questName)
    {
        this.questName = questName;
    }

    public string QuestName() => questName;

    public bool IsCompleted() => isCompleted;

    public void MarkAsCompleted() => isCompleted = true;
}
