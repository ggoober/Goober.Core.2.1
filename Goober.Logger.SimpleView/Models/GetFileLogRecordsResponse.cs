using System.Collections.Generic;

namespace Goober.Logging.SimpleView.Models
{
    public class GetFileLogRecordsResponse
    {
        public string FileFullPath { get; set; }

        public List<Dictionary<string, string>> Records { get; set; } = new List<Dictionary<string, string>>();
    }
}
