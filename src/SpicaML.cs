using System;
using System.Collections.Generic;

using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Antlr.StringTemplate;
using Antlr.StringTemplate.Language;

using Castor;

namespace Spica
{

    public class SpicaML
    {
        protected string input = null;

        protected long time_parse = 0;
        protected long time_process = 0;
        protected long time_resolve = 0;

        protected ITree tree = null;

        protected IList<string> ns = null;
        protected IList<Element> elements = null;

        protected static TypeMap type_map = null;

        protected string vendor = null;

        public string Input     { get { return this.input; } }

        public long TimeParse   { get { return this.time_parse; } }
        public long TimeProcess { get { return this.time_process; } }
        public long TimeResolve { get { return this.time_resolve; } }

        public IList<string>  Namespace     { get { return this.ns; } }
        public ITree          AST           { get { return this.tree; } }
        public IList<Element> Elements      { get { return this.elements; } }
        public string         CurrentVendor { get { return this.vendor; } }

        public IList<Element> Parse(string input)
        {
            this.input = input;

            ICharStream instr = new ANTLRFileStream(input);
            SpicaMLLexer lex = new SpicaMLLexer(instr);
            CommonTokenStream tokens = new CommonTokenStream(lex);
            SpicaMLParser parser = new SpicaMLParser(tokens);

            this.ns = new List<string>();
            this.elements = new List<Element>();

            try {
                DateTime parse_start = DateTime.UtcNow;
                SpicaMLParser.model_return r = parser.model();
                DateTime parse_stop = DateTime.UtcNow;

                this.time_parse += ((parse_stop - parse_start).Ticks / 10000);

                this.tree = r.Tree as ITree;

                if (this.tree == null)
                {
                    Console.Error.WriteLine("SpicaML: Error parsing input!");
                    return null;
                }

                DateTime process_start = DateTime.UtcNow;
                GetNamespace(this.tree);
                ProcessTree(this.tree, input);
                DateTime process_stop = DateTime.UtcNow;

                this.time_process += ((process_stop - process_start).Ticks / 10000);

                // Resolve only structures and modules;
                // enums are primitive by definition
                DateTime resolve_start = DateTime.UtcNow;
                foreach (Element e in this.elements)
                {
                    if ((!(e is Structure)) && (!(e is Module)))
                    {
                        continue;
                    }

                    e.Resolve(this.elements);
                }
                DateTime resolve_stop = DateTime.UtcNow;

                this.time_resolve += ((resolve_stop - resolve_start).Ticks / 10000);
            }
            catch (RecognitionException re)
            {
                Console.Out.WriteLine(re.StackTrace);
            }

            int nenums = 0;
            int nstructs = 0;
            int nmodules = 0;
            foreach (Element e in this.elements)
            {
                if (e is Enum)
                {
                    nenums++;
                }
                else if (e is Structure)
                {
                    nstructs++;
                }
                else if (e is Module)
                {
                    nmodules++;
                }
            }

            Trace.WriteLine("Extracted {0} enums, {1} structures, and {2} modules in {3} ms",
                            nenums, nstructs, nmodules,
                            this.time_parse + this.time_process + this.time_resolve);

            return this.elements;
        }

        protected void ProcessTree(ITree node, string filename)
        {
            for (int i = 0; i < node.ChildCount; i++)
            {
                Process(node.GetChild(i), filename);
            }
        }

        protected void GetNamespace(ITree node)
        {
            bool found = false;

            for (int i = 0; i < node.ChildCount; i++)
            {
                ITree child = node.GetChild(i);

                if (child.Type == SpicaMLLexer.NAMESPACE)
                {
                    if (found == true)
                    {
                        throw new CException("SpicaML: Namespace defined multiple times!");
                    }

                    found = true;

                    for (int j = 0; j < child.ChildCount; j++)
                    {
                        this.ns.Add(child.GetChild(j).Text);
                    }
                }
            }
        }

        protected void Process(ITree node, string filename)
        {
            switch (node.Type)
            {
                case SpicaMLLexer.VENDOR:
                    {
                        if (node.ChildCount != 1)
                        {
                            throw new CException("Unable to extract vendor in {0}!", filename);
                        }

                        this.vendor = node.GetChild(0).Text;
                    }
                    break;

                case SpicaMLLexer.STRUCT:
                    {
                        Structure s = new Structure(node, filename, Namespace);

                        foreach (Element elem in this.elements)
                        {
                            if (elem.Equals(s))
                            {
                                throw new CException("Unable to create structure '{0}', already defined", s.Name);
                            }
                        }

                        this.elements.Add(s);
                    }
                    break;
                    
                case SpicaMLLexer.ENUM:
                    {
                        Enum e = new Enum(node, filename, Namespace);

                        foreach (Element elem in this.elements)
                        {
                            if (elem.Equals(e))
                            {
                                throw new CException("Unable to create enumeration '{0}', already defined", e.Name);
                            }
                        }

                        this.elements.Add(e);
                    }
                    break;
                    
                case SpicaMLLexer.MODULE:
                    {
                        Module m = new Module(node, filename, Namespace);

                        foreach (Element elem in this.elements)
                        {
                            if (elem.Equals(m))
                            {
                                throw new CException("Unable to create module '{0}', already defined", m.Name);
                            }
                        }

                        this.elements.Add(m);
                    }
                    break;

                case SpicaMLLexer.INCLUDE:
                    {
                        ProcessTree(node, node.Text);
                    }
                    break;
            }
        }

        public static TypeMap Map
        {
            set { type_map = value; }
            get { return type_map; }
        }

        public override string ToString()
        {
            return String.Format("{0} -- Parse: {1} ms, Process: {2} ms, Resolve: {3} ms",
                                 this.input, this.time_parse, this.time_process, this.time_resolve);
        }
    }
}

