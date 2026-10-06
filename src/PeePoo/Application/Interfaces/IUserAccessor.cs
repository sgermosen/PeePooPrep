namespace Application.Interfaces
{
    public interface IUserAccessor
    {
        /// <summary>The signed-in user's username, or null for anonymous requests.</summary>
        string GetUsername();

        bool IsAdmin();
    }
}
