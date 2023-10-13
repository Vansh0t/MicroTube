namespace MicroTube.Services
{
    public class ServiceResult:IServiceResult
    {
        /// <summary>
        /// An http status code
        /// </summary>
        public int Code { get; }
        /// <summary>
        /// Error message, null if IsError = false
        /// </summary>
        public string? Error { get; }
        /// <summary>
        /// Whether the result operation failed
        /// </summary>
        public bool IsError { get; }
        public ServiceResult(int code)
        {
            Code = code;
            Error = null;
            IsError = false;
        }
        public ServiceResult(int code, string error)
        {
            Code = code;
            Error = error;
            IsError = true;
        }
        /// <summary>
        /// Get success result with code 200
        /// </summary>
        public static ServiceResult Success()
        {
            return new ServiceResult(200);
        }
        /// <summary>
        /// Get failed result with code and error provided
        /// </summary>
        public static ServiceResult Fail(int code, string error)
        {
            return new ServiceResult(code, error);
        }
        /// <summary>
        /// Error shortcut with Code = 500 and message
        /// "Server has encountered an unexpected error while processing the request. Please, try again later."
        public static ServiceResult FailInternal()
        {
            return new ServiceResult(500, "Server has encountered an unexpected error while processing the request. Please, try again later.");
        }
    }
    public class ServiceResult<T> : ServiceResult, IServiceResult<T>
    {
        /// <summary>
        /// An object returned by operation
        /// </summary>
        public T? ResultObject { get; }
        public ServiceResult(int code, T? resultObject) : base(code)
        {
            ResultObject = resultObject;
        }

        public ServiceResult(int code, string error) : base(code, error)
        {
        }

        
        /// <summary>
        /// Get success result with code 200 and ResultObject
        /// </summary>
        public static ServiceResult<T> Success(T resultObject)
        {
            return new ServiceResult<T> (200, resultObject);
        }
        /// <summary>
        /// Get failed result with code and error provided
        /// ResultObject is always set to default
        /// </summary>
        public static new ServiceResult<T> Fail(int code, string error)
        {
            return new ServiceResult<T>(code, error);
        }
        /// <summary>
        /// Error shortcut with Code = 500 and message
        /// "Server has encountered an unexpected error while processing the request. Please, try again later."
        /// </summary>
        public static new ServiceResult<T> FailInternal()
        {
            return new ServiceResult<T>(500, "Server has encountered an unexpected error while processing the request. Please, try again later.");
        }

        /// <summary>
        /// If resultObject exists return success, else fail with nullCaseCode
        /// </summary>
        public static ServiceResult<T> FromResultObject(T? resultObject, int nullCaseCode = 404, string nullCaseError = "Requested object does not exist")
        {
            if (resultObject is null)
                return Fail(nullCaseCode, nullCaseError);
            return Success(resultObject);
        }

    }
}
