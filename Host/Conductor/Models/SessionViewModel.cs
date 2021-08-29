using Newtonsoft.Json;
using SharedHost.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using SharedHost.Models.Session;
using SharedHost.Models.Device;

namespace Conductor.Models
{
    public class SessionViewModel
    {
        public ClientSession clientSession { get; set; }

        public string HostUrl {get;set;}

        public int ClientID {get;set;}

        public bool DevMode {get;set;}


        private string ExportJavaEnum<T>()
        {
            var type = typeof(T);
            var values = Enum.GetValues(type).Cast<T>();
            var dict = values.ToDictionary(e => e.ToString(), e => Convert.ToInt32(e));
            var json = JsonConvert.SerializeObject(dict);
            return json;
        }


        public string ModuleJson
        {
            get
            {
                return ExportJavaEnum<Module>();
            }
        }

        public string HidOpcodeJson
        {
            get
            {
                return ExportJavaEnum<HidOpcode>();
            }
        }
        public string OpcodeJson
        {
            get
            {
                return ExportJavaEnum<Opcode>();
            }
        }

        public string QoEModeJson
        {
            get
            {
                return ExportJavaEnum<QoEMode>();
            }
        }
        public string CodecJson
        {
            get
            {
                return ExportJavaEnum<Codec>();
            }
        }        
    }
}
