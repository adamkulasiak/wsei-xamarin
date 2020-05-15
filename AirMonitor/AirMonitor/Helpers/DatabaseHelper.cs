using AirMonitor.Models;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AirMonitor.Helpers
{
    public class DatabaseHelper
    {
        private readonly string _databasePath;
        private readonly SQLiteAsyncConnection _db;
        public DatabaseHelper()
        {
            _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Database.db");
            _db = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
        }

        public async Task CreateTables()
        {
            await _db.CreateTableAsync<InstallationEntity>();
            await _db.CreateTableAsync<MeasurementEntity>();
            await _db.CreateTableAsync<MeasurementItemEntity>();
            await _db.CreateTableAsync<MeasurementValue>();
            await _db.CreateTableAsync<AirQualityIndex>();
            await _db.CreateTableAsync<AirQualityStandard>();
        }

        public async Task SaveInstalations(List<Installation> installations)
        {
            var objList = new List<InstallationEntity>();
            foreach (var item in installations)
            {
                var installationEntity = new InstallationEntity
                {
                    Address = JsonConvert.SerializeObject(item.Address),
                    Elevation = item.Elevation,
                    IsAirlyInstallation = item.IsAirlyInstallation,
                    Sponsor = JsonConvert.SerializeObject(item.Sponsor),
                    Measurement = JsonConvert.SerializeObject(item.Measurement)
                };
                objList.Add(installationEntity);
            }

            await _db.RunInTransactionAsync(t =>
            {
                t.DeleteAll<InstallationEntity>();
                t.InsertAll(objList, false);
            });
        }

        public async Task SaveMeasurements(List<Measurement> measurements)
        {
            await _db.RunInTransactionAsync(t =>
            {
                t.DeleteAll<MeasurementEntity>();
                t.DeleteAll<MeasurementItemEntity>();
                t.DeleteAll<MeasurementValue>();
                t.DeleteAll<AirQualityIndex>();
                t.DeleteAll<AirQualityStandard>();

                foreach (var item in measurements)
                {
                    t.InsertAll(item.Current.Values, false);
                    t.InsertAll(item.Current.Indexes, false);
                    t.InsertAll(item.Current.Standards, false);
                    var obj = new MeasurementItemEntity
                    {
                        FromDateTime = item.Current.FromDateTime,
                        TillDateTime = item.Current.TillDateTime,
                        Indexes = JsonConvert.SerializeObject(item.Current.Indexes),
                        Standards = JsonConvert.SerializeObject(item.Current.Standards),
                        Values = JsonConvert.SerializeObject(item.Current.Values)
                    };
                    t.Insert(obj);
                    var measurementEntity = new MeasurementEntity { CurrentId = obj.Id };
                    t.Insert(measurementEntity);
                }
            }); 
        }

        public async Task<List<Installation>> GetInstallations()
        {
            var installationsEntity = await _db.Table<InstallationEntity>().ToListAsync();
            var installations = new List<Installation>();
            foreach (var item in installationsEntity)
            {
                var installation = new Installation
                {
                    Id = int.Parse(item.Id),
                    Address = JsonConvert.DeserializeObject<Address>(item.Address),
                    Elevation = item.Elevation,
                    IsAirlyInstallation = item.IsAirlyInstallation,
                    Sponsor = JsonConvert.DeserializeObject<Sponsor>(item.Sponsor),
                    Measurement = JsonConvert.DeserializeObject<Measurement>(item.Measurement)
                };
                installations.Add(installation);
            }
            return installations;
        }
    }
}
