﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backplan.Client.Data
{
    public class TrackedFile
    {
        public int Id { get; set; }
        public IEnumerable<FileActions> Actions { get; set; }
    }
}