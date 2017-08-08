using CardInfoCombiner.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DataGenerator
{
    public class CardInfoComparer : IEqualityComparer<FullCardInfo>
    {
        public bool Equals(FullCardInfo x, FullCardInfo y)
        {
            return x.csv.ClientID == y.csv.ClientID;
        }

        public int GetHashCode(FullCardInfo obj)
        {
            return (int)obj.csv.ClientID;
        }
    }
    class Program
    {
        const int rowCnt = 1000000;
        const string dir = @"D:\TEst";
        static void Main(string[] args)
        {
            var serializer = new XmlSerializer(typeof(List<XmlInfoPart>));
            List<FullCardInfo> fLisst = new List<FullCardInfo>();
            for (int i = 0; i < rowCnt; ++i)
            {
                var tmp = FullCardInfo.CreateNewRand();
                fLisst.Add(tmp);
            }
            fLisst = fLisst.Distinct(new CardInfoComparer()).ToList();
            Random r = new Random();
            var permXList = fLisst.Select(x => x.xml).OrderBy(x => r.Next()).ToList();
            var permCList = fLisst.Select(x => x.csv).OrderBy(x => r.Next()).ToList();

            var cnt = 0;
            while (cnt < rowCnt)
            {
                var batchSize = r.Next(1000, 5000);
                var batch = permXList.Skip(cnt).Take(batchSize);
                using (FileStream fs = new FileStream($"{dir}/{Guid.NewGuid().ToString("N")}.xml", FileMode.CreateNew))
                    serializer.Serialize(fs, batch.ToList());
                cnt += batchSize;
            }

            cnt = 0;
            while (cnt < rowCnt)
            {
                var batchSize = r.Next(1000, 5000);
                var batch = permCList.Skip(cnt).Take(batchSize);
                using (StreamWriter sw = new StreamWriter($"{dir}/{Guid.NewGuid().ToString("N")}.csv"))
                    sw.WriteLine(string.Join(Environment.NewLine, batch));
                cnt += batchSize;
            }
        }



    }
}
