using System;
using System.Collections.Generic;
using System.Text;

namespace Goober.Logging.SimpleView.Models
{
    public class GetFilesListResponse
    {
        public string FullPath { get; set; }

        public string Name { get; set; }

        public string Extension { get; set; }

        public string Directory { get; set; }

        public long LengthInBytes { get; set; }

        public DateTime LastChangedDate { get; set; }
        public DateTime CreatedDate { get; internal set; }
    }
}
