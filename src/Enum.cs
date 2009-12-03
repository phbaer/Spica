using System;
using System.Text;
using System.Collections.Generic;
using Antlr.Runtime.Tree;
using Castor;

namespace Spica
{
    public class Enum : Element
    {
        protected int supertype = -1;
        protected ListDictionary<string, string> elements = null;

        public Enum(ITree node, string filename, IList<string> ns) : base(node, filename, ns)
        {
            // Sanilty checks
            if ((node == null) || (node.Type != SpicaMLLexer.ENUM))
            {
                throw new CException("Enum: Unable to create enum, {0}!",
                                     (node == null ? "no node passed" : "no struct node passed"));
            }

            // An enum consists of at least two nodes: the typename and the primitive type
            if (node.ChildCount < 2)
            {
                throw new CException("SpicaML: Unable to create enum, too few child nodes!");
            }

            this.elements = new ListDictionary<string, string>();

            // Scan the children of this structure and extract all relevant data
            for (int j = 0; j < node.ChildCount; j++)
            {
                switch (node.GetChild(j).Type)
                {
                    // Extract name of this structure, should be child 0
                    case SpicaMLLexer.TYPENAME:
                        Name = GetTypeName(node.GetChild(j));
                        break;
                
                    case SpicaMLLexer.ITEM: // Should be childs 2..n
                        {
                            ITree child = node.GetChild(j);

                            if (child.ChildCount < 2)
                            {
                                throw new CException("SpicaML: Unable to extract enumeration item for {0}!", this.Name);
                            }

                            this.elements.Add(child.GetChild(0).Text, child.GetChild(1).Text);
                        }
                        break;
                }
            }

            this.supertype = node.GetChild(1).Type; // The primitive type is child 1 

            if (Name == null)
            {
                throw new CException("Enum: Unable to create structure, no name given!");
            }
        }

        public override string SpicaElementName
        {
            get { return "Enum"; }
        }

        internal override bool Resolved
        {
            get { return true; }
        }

        internal override void Resolve(IList<Element> elements)
        {
            // Nothing to be implemented
        }

        public string SuperType
        {
            get { return TypeString(this.supertype); }
        }

        internal void SetSuperType(int type)
        {
            this.supertype = type;
        }

        public ListDictionary<string, string> Elements
        {
            get { return this.elements; }
        }

        internal void AddElement(string name, string value)
        {
            this.elements.Add(name, value);
        }

        public override string ToString()
        {
            return String.Format("enum {0} (inherits {1}, hash {2}, {3} elements)", Name, SuperType, Hash, this.elements.Count);
        }
    }
}
