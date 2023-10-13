namespace MicroTube.Services
{
    public interface IServiceResult
    {
        public int Code { get; }
        public string? Error { get; }
        public bool IsError { get; }
    }
    public interface IServiceResult<T> : IServiceResult
    {
        public T? ResultObject { get; }
    }
}
