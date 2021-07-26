using SharedHost.Models;
using SlaveManager.Data;
using SlaveManager.Models;
using SlaveManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaveManager.Administration
{
    public interface IAdmin
    {
        public Task<bool> AskForNewDevice(DeviceInformation information);

        public Task LogSlaveCommandLine(int slaveID, CommandResult result);
    }

    public class Admin : IAdmin
    {
        private readonly ApplicationDbContext _db;

        public Admin(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> AskForNewDevice(DeviceInformation information)
        {
            Console.WriteLine("implement later");
            return true;
        }

        public async Task LogSlaveCommandLine(int slaveID, CommandResult result)
        {
            Slave machine = _db.Devices.Find(slaveID);
            if (machine == null)
            {
                throw new InvalidOperationException($"Slave device id {slaveID} not found!");
            }

            CommandLog cmdLog = result as CommandLog;
            cmdLog.Machine = machine;

            _db.CommandLogs.Add(cmdLog);
            await _db.SaveChangesAsync();

            return;
        }
    }
}
