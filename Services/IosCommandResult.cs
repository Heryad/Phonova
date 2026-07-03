using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonova.Services
{
    public class IosCommandResult
    {
        public string Result { get; set; }

        public string Exception { get; set; }

        public int ExitCode { get; set; }
    }

}
