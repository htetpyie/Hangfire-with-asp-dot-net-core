using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using LiteDB;
using Microsoft.Extensions.Options;

namespace HangfireDotNetCoreExample.DbService;

public class LiteDbService 
{
    private LiteDatabase _db;

    public LiteDbService(IOptions<LiteDbOption> option)
    {
        CreateConnection(option);
    }

    public T GetOne<T>(Expression<Func<T, bool>> expression)
    {
        return _db.GetCollection<T>().FindOne(expression);
    }

    public List<T> GetList<T>()
    {
        return _db.GetCollection<T>()
            .FindAll()
            .ToList();
    }

    public void Insert<T>(T model)
    {
        _db.GetCollection<T>()
            .Insert(model);
    }

    public bool Update<T>(T model)
    {
        return _db.GetCollection<T>()
            .Update(model);
    }

    public bool Delete<T>(BsonValue id)
    {
        return _db.GetCollection<T>()
            .Delete(id);
    }

    private void CreateConnection(IOptions<LiteDbOption> option)
    {
        string folderPath = Path.Combine(
            AppDomain
                .CurrentDomain
                .BaseDirectory, option.Value.DatabaseLocation);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        _db = new LiteDatabase(folderPath+"/app.db");
    }
}

public class LiteDbOption
{
    public string DatabaseLocation { get; set; }
}