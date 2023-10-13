namespace MicroTube.Data.Access
{
    public interface IDataAccess
    {
        public void BeginAtomic();
        public void EndAtomic(bool commit);
    }
}
