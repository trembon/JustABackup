﻿using JustABackup.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JustABackup.Models
{
    public class ListJobsModel
    {
        public List<BackupJob> Jobs { get; set; }
    }
}
