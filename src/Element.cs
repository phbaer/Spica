using System;
using System.Text;
using System.Collections.Generic;

using Antlr.Runtime.Tree;

using Castor;

namespace Spica
{
    public abstract class Element
    {
        private string name = null;

        private IList<string> fullname = null;
        private string fullname_string = null;

        private uint id = 0;

        private IList<string> root_ns = null;
        private IList<string> ns = null;
        private string root_ns_string = null;
        private string ns_string = null;

        private int line = 0;
        private int charpos = 0;
        private string filename = null;

        private string package = null;

        /**
         * Default constructor. May be used only for generating custom messages
         * not derived from an ANTLR node (such as, e.g. ROS messages)
         * @param filename The name of the file in which the node is defined
         * @param package The name of the package in which the node is defined
         */
        public Element(string filename, string package)
        {
            this.line = 0;
            this.charpos = 0;
            this.filename = filename;

            RootNamespace = new List<string>();
            Package = package;
        }

        /**
         * Constructor. Used to pass the node to be processed and the
         * filename in which this node is defined.
         * @param node The node to be processed
         * @param filename The name of the file in which the node is defined
         * @param package The name of the package in which the node is defined
         * @param ns The namespace of the element
         */
        public Element(ITree node, string filename, string package, IList<string> ns)
        {
            this.line = node.Line;
            this.charpos = node.CharPositionInLine;
            this.filename = filename;

            RootNamespace = ns;
            Package = package;
        }

       /**
         * Property for accessing the name of the file in which the element is defined.
         */
        public string Filename
        {
            get { return this.filename; }
        }

        /**
         * Property for accessing the root namespace of the element.
         */
        public IList<string> RootNamespace
        {
            get { return this.root_ns; }
            private set
            {
                this.root_ns = value;

                // Generate ns string
                this.root_ns_string = "";
                foreach (string s in this.root_ns)
                {
                    this.root_ns_string += '/';
                    this.root_ns_string += s;
                }
            }
        }

        /**
         * Property for accessing the package name
         */
        public string Package
        {
            get { return this.package; }
            protected set
            {
                this.package = value;

                // Set the namespace
                this.ns = new List<string>(this.root_ns);
                this.ns_string = this.root_ns_string;

                if (this.package != null)
                {
                    this.ns.Add(this.package);
                    this.ns_string += (value != null ? ("/" + value) : "");
                }
            }
        }

        /**
         * Property for accessing the name of the element. The set operation will
         * furthermore update the module's identifier.
         */
        public string Name
        {
            get { return this.name; }
            internal set
            {
                this.name = value;

                UpdateFullName();

                Castor.Jenkins96 hash = new Castor.Jenkins96();
                this.id = hash.ComputeHash(Encoding.ASCII.GetBytes(this.fullname_string));
            }
        }

        /**
         * Property for accessing the full name of the element
         */
        public IList<string> FullName
        {
            get { return this.fullname; }
            internal set { this.fullname = value; }
        }

        /**
         * Property for accessing the full name of the element as a string
         */
        public string FullNameString
        {
            get { return this.fullname_string; }
            internal set { this.fullname_string = value; }
        }

        public string RootNamespaceString
        {
            get { return this.root_ns_string; }
        }

        /**
         * Property for accessing the full namespace of the element
         */
        public IList<string> Namespace
        {
            get { return this.ns; }
        }

        public string NamespaceString
        {
            get { return this.ns_string; }
        }

        /**
         * Returns the name of the Spica element (Module, Struct, ...)
         */
        public abstract string SpicaElementName { get; }

        /**
         * Property for accessing the element's hash.
         */
        public uint Id
        {
            get { return this.id; }
        }

        /**
         * Property for accessing the element's resolved state. Returns true if all
         * relevant references have been resolved, false otherwise.
         */
        internal abstract bool Resolved { get; }

        /**
         * Trigger resolution of all relevant internal references.
         * @param elements List of all available elements that may be
         * relevant for resolving references
         */
        internal abstract void Resolve(IList<Element> elements);

        /**
         * Property for getting information on where the element was defined
         */
        public string Details
        {
            get
            {
                return String.Format("{0}:{1},{2}",
                                     this.filename, this.line, this.charpos);
            }
        }

        /**
         * Maps a primitive type id to a string.
         * @param type The type id of a primitive type as defined my the SpicaML grammar
         * @return A string representing the type
         */
        protected string TypeString(int type)
        {
            if (type == -1)
            {
                return null;
            }

            switch (type)
            {
                case SpicaMLLexer.BOOL:     return "bool";
                case SpicaMLLexer.INT8:     return "int8";
                case SpicaMLLexer.INT16:    return "int16";
                case SpicaMLLexer.INT32:    return "int32";
                case SpicaMLLexer.INT64:    return "int64";
                case SpicaMLLexer.UINT8:    return "uint8";
                case SpicaMLLexer.UINT16:   return "uint16";
                case SpicaMLLexer.UINT32:   return "uint32";
                case SpicaMLLexer.UINT64:   return "uint64";
                case SpicaMLLexer.FLOAT:    return "float32";
                case SpicaMLLexer.FLOAT32:  return "float32";
                case SpicaMLLexer.DOUBLE:   return "float64";
                case SpicaMLLexer.FLOAT64:  return "float64";
                case SpicaMLLexer.TIME:     return "time";
                case SpicaMLLexer.DURATION: return "duration";
                case SpicaMLLexer.STRING:   return "string";
// Type "address" is no longer supported
//                case SpicaMLLexer.ADDRESS: return "address";
            }

            return "undefined";
        }

        /**
         * Maps a string to a primitive type id
         * @param type The type string
         * @return primitive type as defined by the ANTLR grammar, -1 if an error occured
         */
        protected int TypeID(string type)
        {
            if (type == null)
            {
                return -1;
            }

            switch (type)
            {
                case "bool":     return SpicaMLLexer.BOOL;
                case "int8":     return SpicaMLLexer.INT8;
                case "int16":    return SpicaMLLexer.INT16;
                case "int32":    return SpicaMLLexer.INT32;
                case "int64":    return SpicaMLLexer.INT64;
                case "uint8":    return SpicaMLLexer.UINT8;
                case "uint16":   return SpicaMLLexer.UINT16;
                case "uint32":   return SpicaMLLexer.UINT32;
                case "uint64":   return SpicaMLLexer.UINT64;
                case "float":    return SpicaMLLexer.FLOAT;
                case "float32":  return SpicaMLLexer.FLOAT32;
                case "double":   return SpicaMLLexer.DOUBLE;
                case "float64":  return SpicaMLLexer.FLOAT64;
                case "time":     return SpicaMLLexer.TIME;
                case "duration": return SpicaMLLexer.DURATION;
                case "string":   return SpicaMLLexer.STRING;
            }

            return -1;
        }



        /**
         * Returns the text of a TYPENAME node of an ANTLR AST tree
         * @param node The TYPENAME node
         * @return null, if node == null or node.Type != TYPENAME, the string otherwise
         */
        protected string GetTypeName(ITree node)
        {
            return GetTextFromSubNode(SpicaMLLexer.TYPENAME, node);
        }

        /**
         * Returns the text of a NAME node of an ANTLR AST tree
         * @param node The NAME node
         * @return null, if null == null or node.Type != NAME, the string otherwise
         */
        protected string GetName(ITree node)
        {
            return GetTextFromSubNode(SpicaMLLexer.NAME, node);
        }

        /**
         * Returns the text of a node with a specific type below a given node of an ANTLR AST tree
         * @param type The type the requested subnode has to be an instance of
         * @param node The node of which the subnode's text will be returned
         * @return null, if node == null, node.Type != TYPENAME, or the subnode has no children.
         * The subnode's text otherwise.
         */
        protected string GetTextFromSubNode(int type, ITree node)
        {
            // Sanity checks
            if ((node == null) || (node.Type != type))
            {
                return null;
            }

            // We need children!
            if (node.ChildCount == 0)
            {
                return null;
            }

            return node.GetChild(0).Text;
        }

        /**
         * Tests two elements for equality based on their type and their name hashes.
         * @param other A reference to another object
         * @return Returns true, if the two instance represent the same element, false otherwise.
         */
        public override bool Equals(object other)
        {
            return ((other != null) && (other is Element)
                ? (Id == (other as Element).Id)
                : false);
        }

        /**
         * Returns the element's hash code (based on the name).
         * @return int The element's hash code (based on its name)
         */
        public override int GetHashCode()
        {
            return (int)Id;
        }

        /**
         * String representation of the current element
         */
        public override string ToString()
        {
            return String.Format("{0}({1}):{2}({3})", SpicaElementName, GetHashCode(), FullNameString, Id);
        }

        protected abstract void UpdateFullName();
    }
}

