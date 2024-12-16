namespace ProniaApplication.Utilities.Exceptions
{
    public class NotFoundException:Exception
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException() : base("404 Not Found!") { }
        

    }
}
