namespace HfsChargesContainer.Domain.Exceptions
{
    public class ResourceCannotBeFoundException : Exception
    {
        public ResourceCannotBeFoundException(string message) : base(message) {}
    }
}
