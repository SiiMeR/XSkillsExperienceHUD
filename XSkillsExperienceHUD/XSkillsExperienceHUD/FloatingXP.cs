namespace XSkillsExperienceHUD;

public class FloatingXP
{
    private readonly float initialTime;
    public float Alpha;
    public string Text;
    public float TimeToLive; // in seconds
    public double X;
    public double Y;

    public FloatingXP(string text, double x, double y, float timeToLive = 4.0f)
    {
        Text = text;
        X = x;
        Y = y;
        Alpha = 1f;
        TimeToLive = timeToLive;
        initialTime = timeToLive;
    }

    public bool IsDead => TimeToLive <= 0;

    public void Update(float dt)
    {
        // Move upwards
        Y -= dt * 25;

        // Fade out over time
        TimeToLive -= dt;
        Alpha = TimeToLive / initialTime;
    }
}