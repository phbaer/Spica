using System;
using System.Collections.Generic;
using Antlr.Runtime.Tree;
using Castor;

namespace Spica
{
    public class Field : Element
    {
        protected int type = -1;
        protected Element element = null;
        protected string typename = null;

        /**
         * Extracts a field from the given node. The node has to be of type FIELD
         * and contain at least child nodes of type TYPENAME (or a primitive type field)
         * and NAME the typical structure.
         * @param node The node that points to a FIELD node
         * @param filename The name of the file in which the node is defined
         * @return A new instance of a Field
         */
        public Field(ITree node, string filename, IList<string> ns) : base(node, filename, ns)
        {
            // Sanity check
            if ((node == null) || (node.Type != SpicaMLLexer.FIELD))
            {
                throw new CException("Field: Unable to create field, {0}!",
                                     (node == null ? "no node passed" : "no field node passed"));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                switch (node.GetChild(i).Type)
                {
                    case SpicaMLLexer.TYPENAME: // Should be child 0 for compex types
                        this.typename = GetTypeName(node.GetChild(i));
                        break;

                    case SpicaMLLexer.NAME: // Should be childs 1..n
                        Name = GetName(node.GetChild(i));
                        break;
                        
                    default: // Should be child 0 for primitive types
                        this.type = node.GetChild(i).Type;
                        break;
                }
            }

            if (Name == null)
            {
                throw new CException("Field: Unable to create field, no name given!");
            }

            // Sanity check
            if ((this.type == -1) && (this.typename == null))
            {
                throw new CException("Field: Unable to create field {0}, no type given!", Name);
            }
        }

        public override string TypeName
        {
            get { return "Field"; }
        }

        public bool Primitive
        {
            get { return (this.type != -1); }
        }

        internal override bool Resolved
        {
            get { return ((this.type != -1) || (this.element != null)); }
        }

        internal override void Resolve(IList<Element> elements)
        {
            // Primitive type, no need to resolve the type
            if (this.type != -1)
            {
                return;
            }

            if ((elements == null) && (this.element == null))
            {
                throw new CException("Unable to resolve field type '{0}', no viable selection of elements available", this.typename);
            }

            foreach (Element e in elements)
            {
                if (this.typename.Equals(e.Name))
                {
                    this.element = e;
                    return;
                }
            }

            throw new CException("Unable to resolve field type '{0}'", this.typename);
        }

        public override string ToString()
        {
            string typename = TypeString(this.type);

            return String.Format("{0} {1}", (typename == null ? this.typename : typename), Name);
        }
    }
}

