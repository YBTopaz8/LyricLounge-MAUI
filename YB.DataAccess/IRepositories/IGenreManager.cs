using YB.Models;

namespace YB.DataAccess.IRepositories;

public interface IGenreManager
{
    Task AddGenreAsync(GenreModel genre);
    Task<List<GenreModel>> GetAllGenresAsync();
    Task<GenreModel> GetGenreByIdAsync(string id);
}
