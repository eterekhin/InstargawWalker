#nullable enable
namespace AuthProject.EmailSender
{
    public class HandlerResult<TSuccess, TFailed>
        where TSuccess : class
        where TFailed : class
    {
        public bool IsSuccess => Success != null;
        public TSuccess? Success { get; set; }
        public TFailed? Failed { get; set; }
    }
}