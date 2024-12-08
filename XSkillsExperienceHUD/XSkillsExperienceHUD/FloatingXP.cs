public class FloatingXP
{
    private readonly float initialTime;
    public float Alpha;
    public float TimeToLive; // in seconds
    public double X;
    public double Y;

    public FloatingXP(string skillName, float xpValue, double x, double y, float timeToLive = 4.0f)
    {
        SkillName = skillName;
        XpValue = xpValue;
        X = x;
        Y = y;
        Alpha = 1f;
        TimeToLive = timeToLive;
        initialTime = timeToLive;
    }

    public string SkillName { get; }
    public float XpValue { get; private set; }
    public string Text => $"+{XpValue:0.###} {SkillName} XP";

    public bool IsDead => TimeToLive <= 0;

    public void Update(float dt)
    {
        // Move upwards
        Y -= dt * 30;

        // Fade out over time
        TimeToLive -= dt;
        Alpha = TimeToLive / initialTime;
    }

    public void AddXP(float amount)
    {
        XpValue += amount;
    }
}