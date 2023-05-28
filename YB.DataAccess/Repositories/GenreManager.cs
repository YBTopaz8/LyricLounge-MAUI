using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YB.DataAccess.IRepositories;
using YB.Models;

namespace YB.DataAccess.Repositories;

public class GenreManager : IGenreManager
{
    public Task AddGenreAsync(GenreModel genre)
    {
        throw new NotImplementedException();
    }

    public Task<List<GenreModel>> GetAllGenresAsync()
    {
        throw new NotImplementedException();
    }

    public Task<GenreModel> GetGenreByIdAsync(string id)
    {
        throw new NotImplementedException();
    }
}
