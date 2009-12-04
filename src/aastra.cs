/*
 * $Id: AASTra.cs 286 2008-03-21 00:52:14Z phbaer $
 *
 *
 * 2006-2008 Carpe Noctem, Distributed Systems Group, University of Kassel, Germany.
 * 2009 DFKI Philipp A. Baer, DFKI RIC Bremen, Germany.
 *
 * The code is derived from the software contributed to Carpe Noctem by
 * the Carpe Noctem Team.
 *
 * The code is licensed under the Carpe Noctem Userfriendly BSD-Based
 * License (CNUBBL). Redistribution and use in source and binary forms,
 * with or without modification, are permitted provided that the
 * conditions of the CNUBBL are met.
 *
 * You should have received a copy of the CNUBBL along with this
 * software. The license is also available on our website:
 * http://carpenoctem.das-lab.net/license.txt
 *
 *
 * The AAS Translator
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using Antlr.StringTemplate;
using Antlr.StringTemplate.Language;
using Castor;
using Castor.Dynamic;

namespace Spica
{
    using ROS;

    public class Aastra
    {
        protected const string GENERATOR_NAME = "aastra2";
        protected const string GENERATOR_VERSION = "1.9.1";
        protected static readonly string[] GENERATOR_COPYRIGHT = {
            "2009 Spica Robotics Project (http://spica-robotics.net/)",
            "2009 DFKI RIC Bremen (http://robotik.dfki-bremen.de/)",
            "2005-2009 DAS-Lab, University of Kassel (http://das-lab.net/)"
        };
        protected const string GENERATOR_ID = GENERATOR_NAME + "/" + GENERATOR_VERSION;

        protected bool show_stats = false;
        protected string root_path = null;
        protected string template_path = null;
        protected string output_path = null;
        protected bool output_flat = false;
        protected IList<string> template_dirs = null;
        protected List<string> template_targets = null;
        protected IDictionary<string, IList<TemplateInfo>> templates = null;

        protected SystemConfig sc = new SystemConfig();

        protected DateTime start;
        protected DateTime startParse;
        protected DateTime stopParse;
        protected DateTime startTransform;
        protected DateTime stopTransform;
        protected DateTime startGenerate;

        protected ArrayList locatedProperties = new ArrayList();

        protected MsgSwitch trace = new MsgSwitch(MsgLevel.Info);

        protected Queue<ElementInfo> process_elements = null;
        protected struct ElementInfo
        {
            public ElementInfo(string t, IList<TemplateInfo> tpl, Element e, string f)
            {
                target = t;
                templates = tpl;
                element = e;
                filename = f;
            }

            public string target;
            public IList<TemplateInfo> templates;
            public Element element;
            public string filename;
        }

        protected struct TemplateInfo
        {
            public TemplateInfo(string ext, TypeMap type_map, StringTemplateGroup stg)
            {
                extension = ext;
                map = type_map;
                group = stg;
            }

            public StringTemplate GetTemplate(Element element, string filename)
            {
                SpicaML.Map = map;

                StringTemplate template = (StringTemplate)group.GetInstanceOf("main");
                template.PassThroughAttributes = true;
                template.SetAttribute("e", element);
                template.RegisterAttributeRenderer(typeof(string), new StringRenderer());
    
                template.DefineFormalArgument("generator_source_file");
                template.SetAttribute("generator_source_file", filename);
                
                template.DefineFormalArgument("generator_name");
                template.SetAttribute("generator_name", GENERATOR_NAME);
                
                template.DefineFormalArgument("generator_version");
                template.SetAttribute("generator_version", GENERATOR_VERSION);
                
                template.DefineFormalArgument("generator_copyright");
                template.SetAttribute("generator_copyright", GENERATOR_COPYRIGHT);
                
                template.DefineFormalArgument("generator_id");
                template.SetAttribute("generator_id", GENERATOR_ID);

                return template;
            }

            public string extension;
            public TypeMap map;
            public StringTemplateGroup group;
        }

        protected void PrintVersionInformation()
        {
            Console.WriteLine("Spica Robotics {0} {1}", GENERATOR_NAME, GENERATOR_VERSION);
            foreach (string copy in GENERATOR_COPYRIGHT)
            {
                Console.WriteLine("* {0}", copy);
            }
            Debug.WriteLine("Root directory:     {0}", this.root_path);
            Debug.WriteLine("Template directory: {0}", this.template_path);
            if (this.output_path != null)
            {
                Debug.WriteLine("Output directory:   {0}", this.output_path);
            }
        }

        public Aastra(string[] args)
        {
            this.start = DateTime.UtcNow;

            DeterminePaths();
            string target_string = DetermineTargets();

            Arguments a = new Arguments(1, -1);
            a.SetOption("generate!", "Generate code for the given platforms (" + target_string + ")");
            a.SetOption("?output:autogen", "Output directory for generated code, for each target a separate subdirectory is created");
            a.SetOption("?templates", "Directory to search for templates (" + this.template_path + ")");
            a.SetOption("?show-ast", "Show AST after parsing the spec");
            a.SetOption("?show-structs", "Show extracted structures");
            a.SetOption("?show-enums", "Show extracted structures");
            a.SetOption("?show-modules", "Show extracted structures");
            a.SetOption("?show-stats", "Show statistics");
            a.SetOption("?ros-messages", "Process messages defined in ROS");
            a.SetOption("?ros-package-path!", "Path to ROS packages (will be searched recursively)");
            a.SetOption("?flat", "Do not store generated code in type-secific subdirectories (enum, struct, module)");
            a.SetOption("?verbose", "Be verbose, output everything");
            a.SetOption("?quiet", "Suppress all output");

            // Consume the arguments
            try {
                a.Consume(args);
            } catch (Exception e) {
                PrintVersionInformation();
                Console.Error.Write(a.ToString());
                Debug.WriteLine(e.Message);
                Debug.WriteLine("Add one or more specification files");
                Environment.Exit(1);
            }

            // Check the parameter count
            if (a.GetParameters().Count < 1) {
                PrintVersionInformation();
                Console.Error.Write(a.ToString());
                Debug.WriteLine("No input file given!");
                Debug.WriteLine("And one or more specification files");
                Environment.Exit(1);
            }

            bool verbose = a.OptionIsSet("verbose");
            bool quiet = a.OptionIsSet("quiet");
            bool show_enums = a.OptionIsSet("show-enums");
            bool show_structs = a.OptionIsSet("show-structs");
            bool show_modules = a.OptionIsSet("show-modules");

            this.show_stats = a.OptionIsSet("show-stats");
            this.output_path = Path.GetFullPath(a.GetOptionValues("output")[0]);
            this.output_flat = a.OptionIsSet("flat");

            IList<string> targets = a.GetOptionValues("generate");

            foreach (string t in targets)
            {
                if (!this.template_targets.Contains(t))
                {
                    throw new CException("Unknown target {0}, unable to generate code!", t);
                }
            }

            if (targets.Count == 0)
            {
                throw new CException("No targets to generate!");
            }

            this.trace.Level = (quiet ? MsgLevel.Off : (verbose ? MsgLevel.Verbose : MsgLevel.Info));

            if (!quiet)
            {
                PrintVersionInformation();
            }

            DateTime overall_start = DateTime.UtcNow;

            if (a.OptionIsSet("ros-messages"))
            {
                ROS.ROS ros = new ROS.ROS();

                if (a.OptionIsSet("ros-package-path"))
                {
                    ros.AddPackagePath(Path.GetFullPath(a.GetOptionValues("ros-package-path")[0]));
                }

                ros.Scan();
            }

            try
            {
                // Generate the parse tree
                IList<SpicaML> specs = Load(a.GetParameters().ToArray());

                if (a.OptionIsSet("show-ast"))
                {
                    foreach (SpicaML sml in specs)
                    {
                        Trace.WriteLine("Input: {0}", sml.Input);
                        Console.WriteLine(sml.AST.ToString());
                        Console.WriteLine();
                    }
                }

                if (show_structs || show_enums || show_modules)
                {
                    foreach (SpicaML sml in specs)
                    {
                        Trace.WriteLine("Input: {0}", sml.Input);
                        foreach (Element e in sml.Elements)
                        {
                            if (show_structs && (e is Structure))
                            {
                                Console.WriteLine("  {0}", e.ToString());
                            }

                            if (show_enums && (e is Enum))
                            {
                                Console.WriteLine("  {0}", e.ToString());
                            }
 
                            if (show_modules && (e is Module))
                            {
                                Console.WriteLine("  {0}", e.ToString());
                            }
                        }
                    }
                }

                if (this.show_stats)
                {
                    foreach (SpicaML sml in specs)
                    {
                        Trace.WriteLine(sml.ToString());
                    }
                }

                // Load all required templates
                LoadTemplates(targets, specs);

                // Generate the code
                Generate(targets, specs);

            }
            catch (Exception e)
            {
                DisplayException(e);
            }

            DateTime overall_end = DateTime.UtcNow;

            if (this.show_stats)
            {
                Trace.WriteLine("Overall runtime {0} ms", (overall_end - overall_start).Ticks / 10000);
            }
        }

        protected uint ComputeHash(byte[] data)
        {
            const uint p = 16777619;
            uint hash = 2166136261;

            foreach (byte b in data)
            {
                hash = (hash ^ b) * p;
            }

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;

            return hash;
        }

        /**
         * Loads the given SpicaML specifications
         * @param files The files that should be loaded
         * @return A list of SpicaML instances
         */
        protected IList<SpicaML> Load(IList<string> files)
        {
            IList<SpicaML> result = new List<SpicaML>();

            foreach (string file in files)
            {
                SpicaML parser = new SpicaML();

                Trace.WriteLineIf(this.trace.Info, "Reading {0}", file);

                try
                {
                    parser.Parse(file);
                }
                catch (Exception e)
                {
                    throw new Castor.CException(e, "Unable to process {0}", file);
                }

                result.Add(parser);
            }

            return result;
        }

        protected void DeterminePaths()
        {
            // Get path for templates
            this.root_path = Environment.GetEnvironmentVariable("SPICA_ROOT");

            // Environment variable not set, search relative to the assembly path
            if (this.root_path == null)
            {
                this.root_path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                this.root_path = Path.Combine(this.root_path, "..");
            }

            // Sanitise path
            this.root_path = Path.GetFullPath(this.root_path);

            // Check if path exists
            if (!Directory.Exists(this.root_path))
            {
                throw new CException("Unable to find Aastra path ({0})!", this.root_path);
            }

            // Generate template path
            this.template_path = Path.Combine(this.root_path, "templates");
        }

        protected string DetermineTargets()
        {
            // Check if directory exists
            if (!Directory.Exists(this.template_path))
            {
                Debug.WriteLine("Unable to find Aastra templates ({0})!", this.template_path);
                return null;
            }

            this.template_dirs = Directory.GetDirectories(this.template_path);

            // Get list of targets
            this.template_targets = new List<string>();
            foreach (string d in this.template_dirs)
            {
                this.template_targets.Add(Path.GetFileName(d));
            }

            return (this.template_targets.Count > 0 ?
                    String.Join(", ", this.template_targets.ToArray()) :
                    "no templates found");
        }

        protected void LoadTemplates(IList<string> targets, IList<SpicaML> specs)
        {
            IDictionary<string, IList<string>> ext_map = new Dictionary<string, IList<string>>();

            this.templates = new Dictionary<string, IList<TemplateInfo>>();

            foreach (string t in targets)
            {
                string path = Path.Combine(this.template_path, t);

                // Load type mapping
                TypeMap type_map = new TypeMap(path);

                // Get available templates and extensions
                IList<string> files = Directory.GetFiles(path, "*.stg");

                if (files.Count == 0)
                {
                    throw new CException("No templates found in {0}!", path);
                }

                // Get all types and extensions
                IList<string> extensions = new List<string>();
                foreach (string file in files)
                {
                    string filename = Path.GetFileNameWithoutExtension(file);
                    int dot = filename.LastIndexOf('.');

                    string name = filename.Substring(0, dot);
                    string ext = filename.Substring(dot + 1, filename.Length - (dot + 1));

                    if (!ext_map.ContainsKey(name))
                    {
                        ext_map[name] = new List<string>();
                    }

                    ext_map[name].Add(ext);
                }

                // Load the template
                StringTemplateGroup.RegisterGroupLoader(
                        new CommonGroupLoader(ConsoleErrorListener.DefaultConsoleListener, path));

                foreach (SpicaML sml in specs)
                {
                    foreach (Element e in sml.Elements)
                    {
                        if (this.templates.ContainsKey(e.SpicaElementName))
                        {
                            continue;
                        }

                        if (!ext_map.ContainsKey(e.SpicaElementName))
                        {
                            throw new CException("No template found for type {0}!", e.SpicaElementName);
                        }

                        IList<TemplateInfo> ti = new List<TemplateInfo>();
                        foreach (string ext in ext_map[e.SpicaElementName])
                        {
                            try
                            {
                                string tpl_name = String.Format("{0}.{1}", e.SpicaElementName, ext);

                                ti.Add(new TemplateInfo(ext, type_map, StringTemplateGroup.LoadGroup(tpl_name)));
                            }
                            catch (Exception ex) { DisplayException(ex); }
                        }

                        this.templates.Add(e.SpicaElementName, ti);
                    }
                }
            }
        }

        protected void Generate(IList<string> targets, IList<SpicaML> specs)
        {
            int count = 0;

            this.process_elements = new Queue<ElementInfo>();

            foreach (string t in targets)
            {
                string path = Path.Combine(this.template_path, t);

                // Load the template
                StringTemplateGroup.RegisterGroupLoader(
                        new CommonGroupLoader(ConsoleErrorListener.DefaultConsoleListener, path));

                foreach (SpicaML sml in specs)
                {
                    foreach (Element e in sml.Elements)
                    {
                        try
                        {
                            // Load the template group specified by the entry point
                            if (!this.templates.ContainsKey(e.SpicaElementName))
                            {
                                throw new CException("Fatal error: Template {0} not loaded!", e.SpicaElementName);
                            }

                            this.process_elements.Enqueue(new ElementInfo(t, this.templates[e.SpicaElementName], e, sml.Input));

                            count += this.templates[e.SpicaElementName].Count;
                        }
                        catch (Exception ste) { DisplayException(ste); }
                    }
                }
            }

            DateTime generate_start = DateTime.UtcNow;

            IList<Thread> thread_pool = new List<Thread>();
            for (int i = 0; i < System.Environment.ProcessorCount + 1; i++)
            {
                thread_pool.Add(new Thread(ProcessElements));
            }

            Trace.WriteLineIf(this.trace.Verbose, "Created {0} worker threads for {1} jobs", thread_pool.Count, count);

            // Start all threads
            foreach (Thread thread in thread_pool)
            {
                thread.Start();
            }

            // Wait for threads to be finished
            foreach (Thread thread in thread_pool)
            {
                thread.Join();
            }

            DateTime generate_end = DateTime.UtcNow;

            Trace.WriteLine("Generated {0} files in {1} ms", count, (generate_end - generate_start).Ticks / 10000);
        }

        protected void ProcessElements()
        {
            ElementInfo ei;

            for (;;)
            {
                lock (this.process_elements)
                {
                    if (this.process_elements.Count == 0)
                    {
                        return;
                    }

                    ei = this.process_elements.Dequeue();

                    string output = Path.Combine(this.output_path, ei.target);

                    // Create type-specific dir hierarchy
                    if (!this.output_flat)
                    {
                        output = Path.Combine(output, ei.element.SpicaElementName.ToLower());
                    }

                    // Create directory if required
                    if (!Directory.Exists(output))
                    {
                        Directory.CreateDirectory(output);
                    }

                    foreach (TemplateInfo ti in ei.templates)
                    {
                        StringTemplate st = ti.GetTemplate(ei.element, ei.filename);

                        // Create the output file
                        byte[] data = new UTF8Encoding(true).GetBytes(st.ToString());
                        string fn = Path.Combine(output, String.Format("{0}.{1}", ei.element.Name, ti.extension));

                        FileInfo fi = new FileInfo(fn);
        
                        // Check if file content differs
                        if (fi.Exists)
                        {
                            FileStream fso = File.OpenRead(fn);
                            byte[] data_read = new byte[fso.Length];
        
                            fso.Read(data_read, 0, (int)fso.Length);
                            fso.Close();
        
                            uint hash1 = ComputeHash(data);
                            uint hash2 = ComputeHash(data_read);

                            if (hash1 == hash2)
                            {
                                Trace.WriteLineIf(this.trace.Verbose, ". {0}", fn);
                                continue;
                            }
                        }

                        Trace.WriteLineIf(this.trace.Verbose, "> {0}", fn);

                        FileStream fs = File.Create(fn);
                        fs.Write(data, 0, data.Length);
                        fs.Close();
                    }
                }
            }
        }
    
        protected void DisplayException(Exception e)
        {
            int depth = 0;
            Debug.WriteLine("({0}) {1}", depth++, e);
            
            while ((e = e.InnerException) != null)
            {
                Debug.WriteLine("({0}) {1}", depth++, e.Message);
            }
        }

        public static void Main(string[] args)
        {
            new Aastra(args);
        }
    }
}
