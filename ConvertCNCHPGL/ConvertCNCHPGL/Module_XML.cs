using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConvertCNCHPGL
{
    class Module_XML
    {
        private XmlDocument doc;
        private XmlReader reader;
        private XmlNode root;
        private List<XmlNode> nodes;
        
        public string ReadXml(string path, string node)
        {
            doc = new XmlDocument();
            reader = XmlReader.Create(path);
            nodes = new List<XmlNode>();

            doc.Load(reader);
            root = doc.LastChild;
            ChildNodes(root);

            return nodes.Find(x => x.Name == node).InnerText;
        }

        public XmlNode ReturnXml(string path, string node)
        {
            doc = new XmlDocument();
            reader = XmlReader.Create(path);
            nodes = new List<XmlNode>();

            doc.Load(reader);
            root = doc.LastChild;
            ChildNodes(root);

            return nodes.Find(x => x.Name == node);
        }

        private void ChildNodes(XmlNode parent)
        {
            foreach (XmlNode child in parent)
            {
                nodes.Add(child);

                if (child.HasChildNodes)
                    ChildNodes(child);
            }
        }
    }
}
