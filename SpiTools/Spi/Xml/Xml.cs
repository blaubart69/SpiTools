using System.IO;
using System.Collections.Generic;
using System.Xml;

namespace Spi.Xml
{
    public class Tools
    {
        public static IEnumerable<XmlReader> XmlTraverseAllNodesByName(string Nodename, string Filename)
        {
            using (TextReader tr = File.OpenText(Filename))
            {
                using (XmlReader xr = XmlReader.Create(tr))
                {
                    while (xr.ReadToFollowing(Nodename))
                    {
                        yield return xr;
                    }
                }
            }
        }
        public static IEnumerable<IDictionary<string,string>> TraverseNodes(string Nodename, string Filename, string[] AttributesToGet)
        {
            foreach (XmlReader xr in XmlTraverseAllNodesByName(Nodename, Filename))
            {
                IDictionary<string,string> Dic = new Dictionary<string,string>( AttributesToGet.Length );
                foreach (string Attrname in AttributesToGet)
                {
                    Dic.Add( Attrname, xr[Attrname] );
                }
                yield return Dic;
            }
        }
    }
}
