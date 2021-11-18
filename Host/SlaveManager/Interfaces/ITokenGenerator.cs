﻿using SharedHost.Models.User;
using System.Threading.Tasks;
using WorkerManager.SlaveDevices;

namespace WorkerManager.Interfaces
{
    public interface ITokenGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<string> GenerateJwt(ClusterWorkerNode user);

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ClusterWorkerNode?> ValidateToken(string token);
    }
}
