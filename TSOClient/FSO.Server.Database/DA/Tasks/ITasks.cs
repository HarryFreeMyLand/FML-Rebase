﻿using FSO.Server.Database.DA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Database.DA.Tasks
{
    public interface ITasks
    {
        int Create(DbTask task);
        void CompleteTask(int task_id, DbTaskStatus status);
        void SetStatus(int task_id, DbTaskStatus status);
        DbTask CompletedAfter(DbTaskType type, DateTime time);
        PagedList<DbTask> All(int offset = 1, int limit = 20);
    }
}
