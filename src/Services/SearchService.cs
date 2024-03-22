namespace Blog.Services;

public class SearchService
{
    public Action<string?> OnSearch = null!;

    public void Search(string? key)
    {
        OnSearch.Invoke(key);
    }
}
