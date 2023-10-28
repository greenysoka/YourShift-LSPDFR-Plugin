using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using YourShift.Models;

namespace YourShift.Services
{
    public class StatisticsService : ILiteDBService<StatisticModel>
    {

        private string statisticsString = "statistics";

        // Current Bug: LiteDB can't create the Database, to diffrent aproaches :)
        private string GetConnectionString()
        {
            var path = System.IO.Directory.GetCurrentDirectory();
            string fullpath = Path.Combine(path, "statistic.db");
            return fullpath;
        }
        public void Delete(StatisticModel entity)
        {
            var db = new LiteDatabase(GetConnectionString());
            var statistics = db.GetCollection<StatisticModel>(statisticsString);
            statistics.EnsureIndex(x => x.Id);
            var itemToDelete = statistics.FindOne(x => x.Id == entity.Id);
            if (itemToDelete != null)
            {
                statistics.Delete(itemToDelete.Id);
            }
        }

        public void Delete(int id)
        {
            var db = new LiteDatabase(GetConnectionString());
            var statistics = db.GetCollection<StatisticModel>(statisticsString);
            statistics.EnsureIndex(x => x.Id);
            var itemToDelete = statistics.FindOne(x => x.Id == id);
            if (itemToDelete != null)
            {
                statistics.Delete(itemToDelete.Id);
            }
        }

        public StatisticModel Get(StatisticModel entity)
        {
            var db = new LiteDatabase(GetConnectionString());
            var statistc = db.GetCollection<StatisticModel>(statisticsString);
            statistc.EnsureIndex(x => x.Id);
            return statistc.FindById(entity.Id);
        }

        public StatisticModel Get(int id)
        {
            var db = new LiteDatabase(GetConnectionString());
            var statistc = db.GetCollection<StatisticModel>(statisticsString);
            statistc.EnsureIndex(x => x.Id);
            return statistc.FindById(id);
        }

        public List<StatisticModel> GetAll()
        {
            var db = new LiteDatabase(GetConnectionString());

            var collection = db.GetCollection<StatisticModel>(statisticsString);

            List<StatisticModel> statistics = collection.Query().ToList();

            return statistics;
        }

        public void Save(StatisticModel entity)
        {
            var db = new LiteDatabase(GetConnectionString());

            var collection = db.GetCollection<StatisticModel>(statisticsString);

            collection.Insert(entity);

            collection.EnsureIndex(x => x.Id);
        }

        public void Update(StatisticModel entity)
        {
            var db = new LiteDatabase(GetConnectionString());
            var statistics = db.GetCollection<StatisticModel>(statisticsString);
            statistics.EnsureIndex(x => x.Id);
            var itemToDelete = statistics.FindOne(x => x.Id == entity.Id);
            if (itemToDelete != null)
            {
                statistics.Update(entity);
            }
        }
    }
}
