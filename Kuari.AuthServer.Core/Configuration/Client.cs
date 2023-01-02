using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.Core.Configuration
{
    public class Client
    {
        public string Id { get; set; }
        public string  Secret { get; set; }
        public List<String> Audiences { get; set; } //clientların hangi API service'lerine erişim sağlayabileceğini tutacağız.

    }
}
