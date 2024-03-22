namespace Blog.Services;

public class SearchService
{
    public Action<string>? OnSearch;

    public void SearchAsync(string? key)
    {
        OnSearch?.Invoke(key);
    }
}
