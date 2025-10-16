namespace KennyGPT.Interfaces
{
    public interface ISerpAPIService
    {
        Task<string> WebSearch(string query);
    }
}
