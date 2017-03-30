using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WinFormApp
{
    public class XmlData
    {
        private string path;
        private string xmlFile;

        public XmlData()
        {
            var pathPos = Environment.CurrentDirectory.IndexOf("bin");
            path = Environment.CurrentDirectory.Substring(0, pathPos);
            //xmlFile = string.Format(@"{0}Data\Handicap.new.xml", path);
        }

        public List<Hist> GetData(string xmlFile)
        {
            this.xmlFile = xmlFile;
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Hist>));
            using (System.IO.TextReader reader = new System.IO.StreamReader(xmlFile))
            {
                var a= serializer.Deserialize(reader) as List<Hist>;
                return a;
            }
        }

        public void SaveData(List<Hist>  xml)
        {
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(List<Hist>));
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(xmlFile))
            {
                serializer.Serialize(writer, xml);
            }
        }
    }

    public class Hist
    {
        public string Player { get; set; }
        public DateTime DateRound { get; set; }
        public int Score { get; set; }
    }
}
