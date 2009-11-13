using System;
using Castor;
using System.Collections.Generic;
using Antlr.Runtime.Tree;

namespace Spica
{
    public enum DMCType
    {
        Undefined = 0,
        Queue = 1,
        Ringbuffer = 2,
    }

    namespace Annotations
    {

        public abstract class Annotation
        {

            public virtual bool Resolved { get { return true; } }
            public virtual void Resolve(IList<Element> elements) {}
            public abstract string Name { get; }
        }

        public class DMC : Annotation
        {
            protected DMCType type = DMCType.Undefined;
            protected uint size = 0;

            public DMC(string type) : this(type, null)
            {
            }

            public DMC(string type, string size)
            {
                switch (type.ToLower())
                {
                    case "queue":
                        this.type = DMCType.Queue;
                        break;

                    case "ringbuffer":
                        this.type = DMCType.Ringbuffer;
                        break;
                }

                this.size = (size == null
                             ? 0
                             : Convert.ToUInt32(size));
            }

            public DMC(DMCType type, uint size)
            {
                this.type = type;
                this.size = size;
            }

            public DMCType Type { get { return this.type; } }
            public uint Size { get { return this.size; } }

            public override string Name { get { return "DMC"; } }
        }

        public class SourceElement
        {
            protected string protocol = null;
            protected string host = null;
            protected string module = null;

            public SourceElement(string protocol, string host, string module)
            {
                this.protocol = protocol;
                this.host = host;
                this.module = module;
            }

            public string Protocol { get { return this.protocol; } }
            public string Host { get { return this.host; } }
            public string Module { get { return this.module; } }
        }

        public class Source : Annotation
        {
            protected int min = 0;
            protected int max = 0;
            protected IList<SourceElement> elements = null;

            public Source(string min, string max, IList<SourceElement> elements)
            {
                this.min = (min == null
                            ? -1
                            : (min.ToLower() == "n"
                               ? Int32.MaxValue
                               : (int)Convert.ToUInt32(min)));

                this.max = (max == null
                            ? -1
                            : (max.ToLower() == "n"
                               ? Int32.MaxValue
                               : (int)Convert.ToUInt32(max)));

                this.elements = elements;
            }

            public int Min { get { return this.min; } }
            public int Max { get { return this.max; } }
            public int ElementCount { get { return this.elements.Count; } }
            public IList<SourceElement> Elements { get { return this.elements; } }

            public override string Name { get { return "Src/Dst"; } }
        }

        public class Period : Annotation
        {
            protected uint ms = 0;

            public Period(string time)
            {
                if (time.EndsWith("m"))
                {
                    this.ms = Convert.ToUInt32(time.Substring(0, time.Length - 1)) * 60 * 1000;
                }
                else if (time.EndsWith("s"))
                {
                    this.ms = Convert.ToUInt32(time.Substring(0, time.Length - 1)) * 1000;
                }
                else if (time.EndsWith("ms"))
                {
                    this.ms = Convert.ToUInt32(time.Substring(0, time.Length - 2));
                }
                else if (time.EndsWith("us"))
                {
                    this.ms = Convert.ToUInt32(time.Substring(0, time.Length - 2)) / 1000;
                }
                else if (time.EndsWith("ns"))
                {
                    this.ms = Convert.ToUInt32(time.Substring(0, time.Length - 2)) / 10000000;
                }
            }

            public Period(uint ms)
            {
                this.ms = ms;
            }

            public uint Seconds { get { return this.ms / 1000; } }
            public uint MilliSeconds { get { return this.ms; } }

            public override string Name { get { return "Period"; } }
        }

        public class Type : Annotation
        {
            protected Element element = null;

            public Type(Element element)
            {
                this.element = element;
            }

            public Element Element { get { return this.element; } }

            public override string Name { get { return "Type"; } }
        }

        public class Extract : Annotation
        {
            protected Module module = null;
            protected ModuleSubscribe sub = null;

            protected IList<string> field = null;
            protected string typename = null;
            protected Element type = null;
            protected IList<Annotation> annotations = null;

            protected Annotations.DMC dmc = null;
            protected Annotations.Period ttl = null;

            public Extract(Module module, ModuleSubscribe sub, ITree node)
            {
                if (node.Type != SpicaMLLexer.EXTRACT)
                {
                    throw new CException("Annotations.Extract: Unable to create extract request, wrong node type! ({0})", module.Details);
                }
    
                if (node.ChildCount < 2)
                {
                    throw new CException("Annotations.Extract: Unable to create extract request, wrong number of children! ({0})", module.Details);
                }

                this.module = module;
                this.sub = sub;

                this.field = new List<string>();
                this.annotations = new List<Annotations.Annotation>();

                // Set default values
                this.dmc = module.subExtractDefaultDMC;
                this.ttl = module.subExtractDefaultTTL;

                for (int i = 0; i < node.ChildCount; i++)
                {
                    ITree n = node.GetChild(i);

                    switch (n.Type)
                    {
                        case SpicaMLLexer.FIELDSPEC:
                            for (int j = 0; j < n.ChildCount; j++)
                            {
                                this.field.Add(n.GetChild(j).Text);
                            }
                            break;

                        case SpicaMLLexer.TYPE:
                            if (n.ChildCount < 1)
                            {
                                throw new CException("Annotations.Extract: Unable to create extract request, wrong number of children for the type name! ({0})", module.Details);
                            }

                            this.typename = n.GetChild(0).Text;
                            break;

                        case SpicaMLLexer.DMC:
                            this.dmc = module.GetDMC(n);
                            break;

                        case SpicaMLLexer.TTL:
                            this.ttl = module.GetTTL(n);
                            break;
                    }
                }
            }

            public Element Type { get { return this.type; } }

            public override bool Resolved { get { return this.type != null; } }

            public override void Resolve(IList<Element> elements)
            {
                foreach (Element e in elements)
                {
                    if (e.Name.Equals(this.typename))
                    {
                        this.type = e;
                    }
                }

                if (this.type == null)
                {
                    this.type = null;
                    throw new CException("Annotations.Extract: Cannot find given type '{0}'! ({1})", this.typename, this.module.Details);
                }

                // Check if the given field is available in the respective (subscribed) type
                foreach (Element e in elements)
                {
                    if (e.Name.Equals(this.sub.Name))
                    {
                        Element e1 = e;

                        foreach (string s in this.field)
                        {
                            if (!(e1 is Structure))
                            {
                                string typename = this.field[0];
                                for (int i = 1; i < this.field.Count; i++)
                                {
                                    typename += "." + this.field[i];
                                }

                                this.type = null;
                                throw new CException("Annotations.Extract: Primitive fields ({0}) cannot contain fields! ({1} in {2})", s, typename, this.module.Details);
                            }

                            Structure str = e1 as Structure;

                            if (!str.AllFields.Keys.Contains(s))
                            {
                                this.type = null;
                                throw new CException("Annotations.Extract: Unable to find field '{0}' in type '{1}'! ({2})", s, this.sub.Name, this.module.Details);
                            }

                            e1 = str.AllFields[s];
                        }
                    }
                }
            }

            public override string Name { get { return "Extract"; } }
        }
    }
}

