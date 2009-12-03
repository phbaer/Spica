using System;
using System.Text;
using System.Collections.Generic;
using Antlr.Runtime.Tree;
using Castor;

namespace Spica
{

    public class ModuleSubscribe : Element
    {
        protected Module module = null;

        protected Annotations.DMC annDMC = null;
        protected Annotations.Source annSrc = null;
        protected Annotations.Period annPeriod = null;
        protected Annotations.Type annType = null;
        protected Annotations.Period annTTL = null;

        protected IList<Annotations.Extract> extract = null;

        public ModuleSubscribe(Module module, ITree node, string filename, IList<string> ns) :
            base(node, filename, ns)
        {
            if (node.Type != SpicaMLLexer.SUB)
            {
                throw new CException("Module: Unable to create subscription request, wrong node type! ({0})", module.Details);
            }

            if (node.ChildCount < 1)
            {
                throw new CException("Module: Unable to create subscription request, wrong number of children! ({0})", module.Details);
            }

            this.module = module;
            Name = node.GetChild(0).Text;

            this.annDMC = this.module.subDefaultDMC;
            this.annSrc = this.module.subDefaultSrc;
            this.annPeriod = this.module.subDefaultPeriod;
            this.annType = this.module.subDefaultType;
            this.annTTL = this.module.subDefaultTTL;

            this.extract = new List<Annotations.Extract>();

            for (int i = 1; i < node.ChildCount; i++)
            {
                switch (node.GetChild(i).Type)
                {
                    case SpicaMLLexer.DMC:
                        this.annDMC = this.module.GetDMC(node.GetChild(i));
                        break;

                    case SpicaMLLexer.PERIOD:
                        this.annPeriod = this.module.GetPeriod(node.GetChild(i));
                        break;

                    case SpicaMLLexer.SRC:
                        this.annSrc = this.module.GetSource(node.GetChild(i));
                        break;

                    case SpicaMLLexer.TYPE:
                        this.annType = this.module.GetType(node.GetChild(i));
                        break;

                    case SpicaMLLexer.TTL:
                        this.annTTL = this.module.GetTTL(node.GetChild(i));
                        break;

                    case SpicaMLLexer.EXTRACT:
                        this.extract.Add(new Annotations.Extract(this.module, this, node.GetChild(i)));
                        break;
                }
            }
        }

        public override string SpicaElementName
        {
            get { return "ModuleSubscribe"; }
        }

        public Annotations.DMC    DMC    { get { return this.annDMC; } }
        public Annotations.Source Source { get { return this.annSrc; } }
        public Annotations.Period Period { get { return this.annPeriod; } }
        public Annotations.Type   Type   { get { return this.annType; } }
        public Annotations.Period TTL    { get { return this.annTTL; } }

        internal override bool Resolved
        {
            get {
                foreach (Annotations.Extract e in this.extract)
                {
                    if (!e.Resolved) return false;
                }
                return true;
            }
        }

        internal override void Resolve(IList<Element> elements)
        {
            foreach (Annotations.Extract e in this.extract)
            {
                e.Resolve(elements);
            }
        }
    }

    public class ModulePublish : Element
    {
        protected Module module = null;
        protected string name = null;

        protected Annotations.DMC annDMC = null;
        protected Annotations.Source annDst = null;
        protected Annotations.Period annPeriod = null;
        protected Annotations.Period annTTL = null;

        public ModulePublish(Module module, ITree node, string filename, IList<string> ns) :
            base(node, filename, ns)
        {
            if (node.Type != SpicaMLLexer.PUB)
            {
                throw new CException("Module: Unable to create publication request, wrong node type! ({0})", module.Details);
            }

            if (node.ChildCount < 1)
            {
                throw new CException("Module: Unable to create publication request, wrong number of children! ({0})", module.Details);
            }

            this.module = module;
            Name = node.GetChild(0).Text;

            this.annDMC = this.module.pubDefaultDMC;
            this.annDst = this.module.pubDefaultDst;
            this.annPeriod = this.module.pubDefaultPeriod;
            this.annTTL = this.module.pubDefaultTTL;

            for (int i = 1; i < node.ChildCount; i++)
            {
                switch (node.GetChild(i).Type)
                {
                    case SpicaMLLexer.DMC:
                        this.annDMC = this.module.GetDMC(node.GetChild(i));
                        break;

                    case SpicaMLLexer.PERIOD:
                        this.annPeriod = this.module.GetPeriod(node.GetChild(i));
                        break;

                    case SpicaMLLexer.DST:
                        this.annDst = this.module.GetSource(node.GetChild(i));
                        break;

                    case SpicaMLLexer.TTL:
                        this.annTTL = this.module.GetTTL(node.GetChild(i));
                        break;
                }
            }
        }

        public override string SpicaElementName
        {
            get { return "ModulePublish"; }
        }

        public Annotations.DMC    DMC         { get { return this.annDMC; } }
        public Annotations.Source Destination { get { return this.annDst; } }
        public Annotations.Period Period      { get { return this.annPeriod; } }
        public Annotations.Period TTL         { get { return this.annTTL; } }

        internal override bool Resolved { get { return true; } }
        internal override void Resolve(IList<Element> elements) { }
    }


    public class Module : Element
    {
        internal Annotations.DMC subDefaultDMC = new Annotations.DMC(DMCType.Ringbuffer, 1);
        internal Annotations.Source subDefaultSrc = null;
        internal Annotations.Period subDefaultPeriod = new Annotations.Period(33);
        internal Annotations.Type subDefaultType = null;
        internal Annotations.Period subDefaultTTL = new Annotations.Period(5000);

        internal Annotations.DMC pubDefaultDMC = new Annotations.DMC(DMCType.Ringbuffer, 1);
        internal Annotations.Period pubDefaultPeriod = new Annotations.Period(33);
        internal Annotations.Source pubDefaultDst = null;
        internal Annotations.Period pubDefaultTTL = new Annotations.Period(5000);

        internal Annotations.DMC subExtractDefaultDMC = new Annotations.DMC(DMCType.Ringbuffer, 1);
        internal Annotations.Period subExtractDefaultTTL = new Annotations.Period(5000);

        protected IList<ModuleSubscribe> subs = null;
        protected IList<ModulePublish> pubs = null;

        public Module(ITree node, string filename, IList<string> ns) : base(node, filename, ns)
        {
            if ((node == null) || (node.Type != SpicaMLLexer.MODULE))
            {
                throw new CException("Module: Unable to create module, {0}!",
                                     (node == null ? "no node passed" : "no struct node passed"));
            }

            this.subs = new List<ModuleSubscribe>();
            this.pubs = new List<ModulePublish>();

            // Scan the children of this structure and extract all relevant data
            for (int j = 0; j < node.ChildCount; j++)
            {
                switch (node.GetChild(j).Type)
                {
                    // Extract name of this structure
                    case SpicaMLLexer.TYPENAME:
                        Name = GetTypeName(node.GetChild(j));
                        break;

                    // Extract publication defaults
                    case SpicaMLLexer.PUBDEF:
                        switch (node.GetChild(j).Type)
                        {
                            case SpicaMLLexer.DMC:
                                this.pubDefaultDMC = GetDMC(node.GetChild(j));
                                break;

                            case SpicaMLLexer.PERIOD:
                                this.pubDefaultPeriod = GetPeriod(node.GetChild(j));
                                break;

                            case SpicaMLLexer.DST:
                                this.pubDefaultDst = GetSource(node.GetChild(j));
                                break;

                            case SpicaMLLexer.TTL:
                                this.pubDefaultTTL = GetTTL(node.GetChild(j));
                                break;
                        }
                        break;

                    // Extract publications
                    case SpicaMLLexer.PUB:
                        this.pubs.Add(new ModulePublish(this, node.GetChild(j), Filename, Namespace));
                        break;

                    // Extract subscription defaults
                    case SpicaMLLexer.SUBDEF:
                        switch (node.GetChild(j).Type)
                        {
                            case SpicaMLLexer.DMC:
                                this.subDefaultDMC = GetDMC(node.GetChild(j));
                                break;

                            case SpicaMLLexer.PERIOD:
                                this.subDefaultPeriod = GetPeriod(node.GetChild(j));
                                break;

                            case SpicaMLLexer.SRC:
                                this.subDefaultSrc = GetSource(node.GetChild(j));
                                break;

                            case SpicaMLLexer.TYPE:
                                this.subDefaultType = GetType(node.GetChild(j));
                                break;

                            case SpicaMLLexer.TTL:
                                this.subDefaultTTL = GetTTL(node.GetChild(j));
                                break;
                        }
                        break;

                    // Extract subscription extraction defaults
                    case SpicaMLLexer.SUBEXDEF:
                        switch (node.GetChild(j).Type)
                        {
                            case SpicaMLLexer.DMC:
                                this.subExtractDefaultDMC = GetDMC(node.GetChild(j));
                                break;

                            case SpicaMLLexer.TTL:
                                this.subExtractDefaultTTL = GetTTL(node.GetChild(j));
                                break;
                        }
                        break;

                    // Extract subscriptions
                    case SpicaMLLexer.SUB:
                        this.subs.Add(new ModuleSubscribe(this, node.GetChild(j), Filename, Namespace));
                        break;
                }
            }

            if (Name == null)
            {
                throw new CException("Module: Unable to create module, no name given!");
            }
        }

        public override string SpicaElementName
        {
            get { return "Module"; }
        }

        internal override bool Resolved
        {
            get {
                foreach (ModuleSubscribe s in this.subs)
                {
                    if (!s.Resolved) return false;
                }
                return true;
            }
        }

        internal override void Resolve(IList<Element> elements)
        {
            foreach (ModuleSubscribe s in this.subs)
            {
                s.Resolve(elements);
            }
        }

        internal Annotations.DMC GetDMC(ITree node)
        {
            if (node.Type != SpicaMLLexer.DMC)
            {
                throw new CException("Module: Unable to retrieve DMC annotation, wrong node type! ({0})", Details);
            }

            if (node.ChildCount < 2)
            {
                throw new CException("Module: Unable to retrieve DMC annotation! ({0})", Details);
            }

            if (node.ChildCount == 2)
            {
                return new Annotations.DMC(node.GetChild(1).Text);
            }

            return new Annotations.DMC(node.GetChild(1).Text, node.GetChild(2).Text);
        }

        internal Annotations.Period GetPeriod(ITree node)
        {
                if (node.Type != SpicaMLLexer.PERIOD)
            {
                throw new CException("Module: Unable to retrieve period annotation, wrong node type! ({0})", Details);
            }

            if (node.ChildCount < 2)
            {
                throw new CException("Module: Unable to retrieve period annotation! ({0})", Details);
            }

            return new Annotations.Period(node.GetChild(1).Text);
        }

        internal Annotations.Source GetSource(ITree node)
        {
            if ((node.Type != SpicaMLLexer.SRC) && (node.Type != SpicaMLLexer.DST))
            {
                throw new CException("Module: Unable to retrieve source/destination annotation, wrong node type! ({0})", Details);
            }

            string min = null;
            string max = null;

            IList<Annotations.SourceElement> elements = new List<Annotations.SourceElement>();

            for (int i = 0; i < node.ChildCount; i++)
            {
                ITree n = node.GetChild(i);

                switch (n.Type)
                {
                    case SpicaMLLexer.CARD:
                        {
                            int count = n.ChildCount;

                            if (count > 0)
                            {
                                min = n.GetChild(0).Text;
                            }

                            if (count > 1)
                            {
                                max = n.GetChild(1).Text;
                            }
                        }
                        break;

                    case SpicaMLLexer.SPEC:
                        elements.Add(GetSourceElement(n));
                        break;
                        
                    case SpicaMLLexer.NOSPEC:
                        elements.Add(GetSourceElement(n));
                        break;
                }
            }

            return new Annotations.Source(min, max, elements);
        }

        internal Annotations.SourceElement GetSourceElement(ITree node)
        {
            if ((node.Type != SpicaMLLexer.SPEC) && (node.Type != SpicaMLLexer.NOSPEC))
            {
                throw new CException("Module: Unable to retrieve source/destination element annotation, wrong node type! ({0})", Details);
            }

            if ((node.ChildCount < 2) || (node.ChildCount > 3))
            {
                throw new CException("Module: Unable to retrieve source/destination element annotation, wrong number of children! ({0})", Details);
            }

            if (node.ChildCount == 2)
            {
                return new Annotations.SourceElement(null, node.GetChild(0).Text, node.GetChild(1).Text);
            }

            return new Annotations.SourceElement(node.GetChild(0).Text, node.GetChild(1).Text, node.GetChild(2).Text);
        }

        internal Annotations.Period GetTTL(ITree node)
        {
            if (node.ChildCount < 1)
            {
                throw new CException("Module: Unable to retrieve TTL annotation! ({0})", Details);
            }

            return new Annotations.Period(node.GetChild(0).Text);
        }

        internal Annotations.Type GetType(ITree node)
        {
            return null;
        }

        public override string ToString()
        {
            return String.Format("module {0} (hash {1}, {2} pubs, {3} subs) {4}",
                                 Name, Hash, this.pubs.Count, this.subs.Count, (Resolved ? "ok" : "types not resoved"));
        }
    }
}
