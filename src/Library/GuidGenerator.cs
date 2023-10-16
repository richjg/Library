namespace Library
{
    public interface IGuidGenerator
    {
        Guid GenerateGuid();
    }

    public class GuidGenerator : IGuidGenerator
    {
        public Guid GenerateGuid()
        {
            return Guid.NewGuid();
        }
    }
}
