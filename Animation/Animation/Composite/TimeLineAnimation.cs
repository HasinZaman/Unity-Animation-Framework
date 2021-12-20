 public class TimeLineAnimation : Animation
{
    public const string animationType = "TimeLineAnimation";
    public Animation anim;
    public float start;
    public float duration;

    public TimeLineAnimation()
    {
    }

    public TimeLineAnimation(AnimationManager parent, JSON json)
    {
        this.duration = float.Parse(json.getVal("duration").getVal());
        anim = Animation.load(parent, json.getVal("anim"));
    }

    public TimeLineAnimation(Animation anim, float start, float duration)
    {
        this.anim = anim;
        this.start = start;
        this.duration = duration;
    }

    public override void animate(float t)
    {
        anim.animate(t);
    }

    public override Animation Clone()
    {
        return new TimeLineAnimation(this.anim, this.start, this.duration);
    }

    public override void drawMenu()
    {
        anim.drawMenu();
    }

    public override string getJsonData()
    {
        string tmp =
            $"\"type\":\"{TimeLineAnimation.animationType}\"," +
            $"\"duration\":\"{duration}\"," +
            $"\"anim\":{{{anim.getJsonData()}}}";

        return tmp;
    }
}
