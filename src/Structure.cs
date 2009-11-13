using System;
using System.Text;
using System.Collections.Generic;
using Antlr.Runtime.Tree;
using Castor;

namespace Spica
{
    public class Structure : Element
    {
        protected IList<string> supertypeNames = null;
        protected ListDictionary<string, Structure> supertypes = null;
        protected ListDictionary<string, Structure> children = null;
        protected ListDictionary<string, Field> fields = null;

        public Structure(ITree node, string filename, IList<string> ns) : base(node, filename, ns)
        {
            if ((node == null) || (node.Type != SpicaMLLexer.STRUCT))
            {
                throw new CException("Structure: Unable to create structure, {0}!",
                                     (node == null ? "no node passed" : "no struct node passed"));
            }

            this.supertypeNames = new List<string>();
            this.supertypes = new ListDictionary<string, Structure>();
            this.children = new ListDictionary<string, Structure>();
            this.fields = new ListDictionary<string, Field>();

            // Scan the children of this structure and extract all relevant data
            for (int j = 0; j < node.ChildCount; j++)
            {
                switch (node.GetChild(j).Type)
                {
                    // Extract name of this structure
                    case SpicaMLLexer.TYPENAME:
                        Name = GetTypeName(node.GetChild(j));
                        break;

                    case SpicaMLLexer.INHERIT:
                        {
                            ITree child = node.GetChild(j);
                            if ((child.ChildCount > 0) && (child.GetChild(0).Type == SpicaMLLexer.TYPENAME))
                            {
                                string typename = GetTypeName(child.GetChild(0));

                                if (this.supertypeNames.Contains(typename))
                                {
                                    throw new CException("SpicaML: Unable to define super type '{0}' for structure '{1}', already defined!",
                                                         typename, Name);
                                }

                                try
                                {
                                    this.supertypeNames.Add(typename);
                                }
                                catch (Exception e)
                                {
                                    throw new CException(e, "Unable to add super type to structure {0}", Name);
                                }
                            }
                        }
                        break;

                    case SpicaMLLexer.FIELD:
                        {
                            Field f = new Field(node.GetChild(j), Filename, Namespace);

                            try
                            {
                                this.fields.Add(f.Name, f);
                            }
                            catch (Exception e)
                            {
                                throw new CException(e, "Unable to add field '{0}' to structure '{1}'", f.Name, Name);
                            }
                        }
                        break;
                }
            }

            if (Name == null)
            {
                throw new CException("Structure: Unable to create structure, no name given!");
            }
        }

        internal override bool Resolved
        {
            get
            {
                // Check if the supertypes are unresolved
                if (this.supertypeNames.Count > 0)
                {
                    Console.WriteLine("Unresolved super types");
                    return false;
                }

                // Check if *any* field is still unresolved
                lock (this.fields)
                {
                    foreach (Field f in this.AllFields.Values)
                    {
                        if (!f.Resolved)
                        {
                            Console.WriteLine("Unresolved field " + f);
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        internal override void Resolve(IList<Element> elements)
        {
            IList<string> supertypeNamesRemoved = new List<string>();

            lock (this.supertypes)
            {
                // Go through all structure names
                for (int i = 0; i < this.supertypeNames.Count; i++)
                {
                    // Iterate through the given structures
                    foreach (Element e in elements)
                    {
                        if (!(e is Structure))
                        {
                            continue;
                        }

                        Structure s = e as Structure;

                        // If the names are equal, insert the structure reference into the dict
                        if (this.supertypeNames[i].Equals(s.Name))
                        {
                            try
                            {
                                this.supertypes.Add(s.Name, s);
                                s.AddChild(this);
                            }
                            catch (Exception ex)
                            {
                                throw new CException(ex, "Unable to resolve super types for '{0}'", Name);
                            }

                            supertypeNamesRemoved.Add(s.Name);
                        }
                    }
                }
            }

            if (this.supertypeNames.Count != supertypeNamesRemoved.Count)
            {
                throw new Exception("Structure: Unable to resolve all super types for " + Name + "!");
            }

            this.supertypeNames.Clear();

            // Resolve *all* fields
            lock (this.fields)
            {
                foreach (Field f in AllFields.Values)
                {
                    try
                    {
                        f.Resolve(elements);
                    }
                    catch (Exception e)
                    {
                        throw new CException(e, "Unable to resolve field '{0}' in structure '{1}' ({2})",
                                             f.Name, Name, Details);
                    }
                }
            }
        }

        public override string TypeName
        {
            get { return "Struct"; }
        }

        public ListDictionary<string, Structure> SuperTypes
        {
            get { return this.supertypes; }
        }

        public void SetSuperTypes(IList<string> supertypes)
        {
            this.supertypeNames = supertypes;
        }

        public void SetSuperTypes(IList<Structure> supertypes)
        {
            Integrate<Structure>(supertypes, this.supertypes);
        }

        public ListDictionary<string, Structure> Children
        {
            get { return this.children; }
        }

        public void SetChildren(IList<Structure> children)
        {
            Integrate<Structure>(children, this.children);
        }

        internal void AddChild(Structure child)
        {
            this.children.Add(child.Name, child);
        }

        public ListDictionary<string, Field> Fields
        {
            get { return this.fields; }
        }

        public ListDictionary<string, Field> AllFields
        {
            get {
                ListDictionary<string, Field> result = new ListDictionary<string, Field>();

                foreach (Structure s in this.supertypes.Values)
                {
                    foreach (Field f in s.Fields.Values)
                    {
                        if (result.Contains(f.Name))
                        {
                            throw new CException("Structure: '{0}' contains two equally named fields ({1})",
                                                 Name, f.Name);
                        }

                        result.Add(f.Name, f);
                    }
                }

                foreach (Field f in this.fields.Values)
                {
                    if (result.Contains(f.Name))
                    {
                        throw new CException("Structure: '{0}' contains two equally named fields ({1})",
                                             Name, f.Name);
                    }

                    result.Add(f.Name, f);
                }

                return result;
            }
        }

        public void SetFields(IList<Field> fields)
        {
            Integrate<Field>(fields, this.fields);
        }

        protected void Integrate<T>(IList<T> values, ListDictionary<string, T> dict) where T: Element
        {
            if (values != null)
            {
                foreach (T t in values)
                {
                    dict.Add(t.Name, t);
                }
            }
        }

        public override string ToString()
        {
            return String.Format("struct {0} (hash {1}, {2} fields, {3} super types, {4} children) {5}",
                                 Name, Hash, this.fields.Count, this.supertypes.Count, this.children.Count,
                                 (Resolved ? "ok" : "types not resoved"));
        }
    }
}
