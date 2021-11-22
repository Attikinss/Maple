namespace Maple
{
    public interface INoiseListener
    {
        void DetectNoise(object source, float loudness);
    }
}