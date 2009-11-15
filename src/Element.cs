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
        private uint id = 0;

        private IList<string> ns = null;

        private int line = 0;
        private int charpos = 0;
        private string filename = null;

        /**
         * Default constructor. Used to pass the node to be processed and the
         * filename in which this node is defined.
         * @param node The node to be processed
         * @param filename The name of the file in which the node is defined
         */
        public Element(ITree node, string filename, IList<string> ns)
        {
            this.line = node.Line;
            this.charpos = node.CharPositionInLine;
            this.filename = filename;
            this.ns = ns;
        }

       /**
         * Property for accessing the name of the file in which the element is defined.
         */
        public string Filename
        {
            get { return this.filename; }
        }

        /**
         * Property for accessing the namespace for the element.
         */
        public IList<string> Namespace
        {
            get { return this.ns; }
            internal set { this.ns = value; }
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
                if (value != null)
                {
                    Castor.Jenkins96 hash = new Castor.Jenkins96();

                    this.name = value;
                    this.id = hash.ComputeHash(Encoding.ASCII.GetBytes(this.name));
                }
            }
        }

        public abstract string TypeName { get; }

        /**
         * Property for accessing the element's hash.
         */
        public uint Hash
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
                case SpicaMLLexer.BOOL:    return "bool";
                case SpicaMLLexer.INT8:    return "int8";
                case SpicaMLLexer.INT16:   return "int16";
                case SpicaMLLexer.INT32:   return "int32";
                case SpicaMLLexer.INT64:   return "int64";
                case SpicaMLLexer.UINT8:   return "uint8";
                case SpicaMLLexer.UINT16:  return "uint16";
                case SpicaMLLexer.UINT32:  return "uint32";
                case SpicaMLLexer.UINT64:  return "uint64";
                case SpicaMLLexer.FLOAT:   return "float";
                case SpicaMLLexer.DOUBLE:  return "double";
                case SpicaMLLexer.STRING:  return "string";
                case SpicaMLLexer.ADDRESS: return "address";
            }

            return "undefined";
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
                ? (Hash == (other as Element).Hash)
                : false);
        }

        /**
         * Returns the element's hash code (based on the name).
         * @return int The element's hash code (based on its name)
         */
        public override int GetHashCode()
        {
            return (int)Hash;
        }
    }
}

