using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardInfoCombiner.Data
{
    public class FullCardInfo
    {
        private static Random _r;
        static FullCardInfo()
        {
            _r = new Random();
        }
        public XmlInfoPart xml { get; set; }
        public CsvInfoPart csv { get; set; }
        public bool Processed { get; set; }

        public static FullCardInfo CreateNewRand()
        {
            var cid = _r.Next();
            var dt = new DateTime(DateTime.Now.Ticks + _r.Next());
            var res = new FullCardInfo
            {
                xml = new XmlInfoPart { ClientID = cid, PAN = "123123123", ExpiredDate = dt },
                csv = new CsvInfoPart { ClientID = cid, FIO = "12312331123", Tel = "123123124" }
            };
            return res;
        }
    }
    [Serializable]
    public class XmlInfoPart
    {
        public string PAN { get; set; }
        public DateTime ExpiredDate { get; set; }
        public long ClientID { get; set; }
    }

    public class CsvInfoPart
    {
        public long ClientID { get; set; }
        public string FIO { get; set; }
        public string Tel { get; set; }

        public override string ToString()
        {
            return $"{FIO},{Tel},{ClientID}";
        }
    }

}
