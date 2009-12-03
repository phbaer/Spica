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
        protected string package = null;
        protected string field_value = null;

        // array < 0: this type is no array
        // array = 0: this type is a dynamic array
        // array > 0: this type is an array with the given size
        protected int array = -1;

        /**
         * Default constsructor for custom fields.
         * @param field An array of field parameters (package, type, name, array/size, value)
         * @param filename The name of the file in which the field is defined
         */
        public Field(string[] field, string filename) : base(filename)
        {
            this.package = field[0];
            this.typename = field[1];
            this.type = TypeID(field[1]);
            this.element = null;

            if (field[3] == null)
            {
                this.array = -1;
            }
            else if (field[3].Length == 0)
            {
                this.array = 0;
            }
            else
            {
                this.array = Int32.Parse(field[3]);
            }

            this.field_value = field[4];

            Name = field[2];

// TODO: Default value functionality not yet implemented!
//            DefaultValue = fi.value;
        }

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

        public override string SpicaElementName
        {
            get { return "Field"; }
        }

        public bool IsPrimitive
        {
            get { return (this.type != -1); }
        }

        public string FieldType
        {
            get
            {
                if (this.type != -1)
                {
                    return SpicaML.Map.GetNativeType(TypeString(this.type));
                }
                return this.typename;
            }
        }

        public string DefaultValue
        {
            get
            {
                if (this.type != -1)
                {
                    return SpicaML.Map.GetDefaultValue(TypeString(this.type));
                }
                return SpicaML.Map.GetDefaultValue("default");
            }
        }

        public string Package
        {
            get { return this.package; }
        }

        public bool IsArray
        {
            get { return (this.array >= 0); }
        }

        public bool IsDynamicArray
        {
            get { return (this.array == 0); }
        }

        public bool IsStaticArray
        {
            get { return (this.array > 0); }
        }

        public int ArraySize
        {
            get { return this.array; }
        }

        public string FieldValue
        {
            get { return this.field_value; }
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

