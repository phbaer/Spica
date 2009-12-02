/*
 * ROS support library: Package class
 *
 * 2009 Spica Robotics Project (http://spica-robotics.net/)
 * 2009 DFKI RIC Bremen, Germany (http://dfki.de/robotik/)
 *
 * Author: Philipp A. Baer <philipp.baer at dfki.de>
 *
 * The code is derived from the software contributed to Carpe Noctem by
 * the Carpe Noctem Team.
 *
 * Licensed under the FreeBSD license (modified BSD license).
 *
 * You should have received a copy of the license along with this
 * software. The license is also available online:
 * http://spica-robotics.net/license.txt
 */

using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Castor;
using Spica;

namespace Spica.ROS
{

    public class Package : Module
    {
        protected string stack_name = null;

        protected Stack stack = null;
        public Stack Stack
        {
            get { return this.stack; }
        }

        protected IList<Structure> structures = null;
        public IList<Structure> Structures
        {
            get { return this.structures; }
        }

        public Package() : base()
        {
            this.structures = new List<Structure>();
        }

        public Package(string dir) : base(dir)
        {
            string[] elements = dir.Split(new char[] { '/' });

            if (elements.Length <= 1)
            {
                Debug.WriteLine("ROS: Package {0} is not part of a stack", Name);
            }

            this.stack_name = elements[elements.Length - 2];
            this.structures = new List<Structure>();
        }

        public override string ManifestFile
        {
            get { return "manifest.xml"; }
        }

        public override string CacheFile
        {
            get { return ".rospack_cache"; }
        }

        protected override void ProcessingHook(XmlTextReader reader)
        {
            string name = reader.Name.ToLower();

            // Extract dependencies
            if (name.Equals("depend"))
            {
                if (reader.MoveToAttribute("package"))
                {
                    this.dep_names.Add(reader.Value);
                }
            }
        }

        protected override void InitHook()
        {
            string message_dir = Path.Combine(this.dir, "msg");

            if (System.IO.Directory.Exists(message_dir))
            {
//                Debug.WriteLine("ROS: Message definitions found for structure {0}", Name);

                string package = @"(\w*)"; // 0
                string type    = @"(\w*)"; // 1
                string array   = @"(\[(\d+)?\])"; // 2, 3
                string name    = @"(\w*)"; // 4
                string value   = @"(.*)$";

                Regex decl = new Regex(
                        String.Format("^\\s*(?:{0}\\/)?{1}(?:{2})?\\s*{3}(?:\\s*=\\s*{4})?",
                            package, type, array, name, value),
                        RegexOptions.Singleline);

                string[] files = System.IO.Directory.GetFiles(message_dir, "*.msg");

                foreach (string f in files)
                {
                    List<string[]> fields = new List<string[]>();
                    StreamReader reader = new StreamReader(File.OpenRead(f));

                    string line = null;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        // Ignore empty lines and lines that are commented out
                        if (line.Length == 0) continue;
                        if (line.StartsWith("#")) continue;

                        // package, type, name, array(-len), value
                        string[] parts = new string[5];

                        Match m = decl.Match(line);

                        if (m.Success)
                        {
                            if (m.Groups.Count >= 7)
                            {
                                // Package
                                if (m.Groups[1].Captures.Count > 0)
                                {
                                    parts[0] = m.Groups[1].Captures[0].Value;
                                }

                                // Type
                                parts[1] = m.Groups[2].Captures[0].Value;
                                // Name
                                parts[2] = m.Groups[5].Captures[0].Value;

                                // Array and array size if applicable
                                if (m.Groups[3].Captures.Count > 0)
                                {
                                    parts[3] = (
                                            m.Groups[4].Captures.Count > 0 ?
                                            m.Groups[4].Captures[0].Value : "");
                                }
    
                                // Value
                                if (m.Groups[6].Captures.Count > 0)
                                {
                                    parts[4] = SanitiseValue(m.Groups[6].Captures[0].Value);
                                }
                            }
                        }
                        else
                        {
                            throw new CException("ROS: Unable to parse field definition in {0} of package {1} ({2})!", f, Name, line);
                        }

                        fields.Add(parts);
                    }

                    this.structures.Add(new Structure(this.path, fields));
                }
            }

            /*
            Debug.WriteLine("ROS: Structures defined in package {0}:", Name);
            foreach (Structure s in this.structures)
            {
                Debug.WriteLine("+ {0}", s.ToString());
            }
            */
        }

        protected string SanitiseValue(string val)
        {
            bool in_string = false;

            for (int i = 0; i < val.Length; i++)
            {
                if ((val[i] == '\'') || (val[i] == '"'))
                {
                    in_string = !in_string;
                }

                if ((val[i] == '#') && (!in_string))
                {
                    return val.Substring(0, i);
                }
            }

            return val;
        }

        internal bool ResolveStack(IList<Stack> stacks)
        {
            if (this.stack_name == null) return true;

            bool found = false;

            foreach (Stack s in stacks)
            {
                if (s.Name.Equals(this.stack_name))
                {
                    this.stack = s;

                    found = true;
                }
            }

            if (!found)
            {
                throw new CException("Unable to resolve stack {0} in Package/{1}", this.stack_name, Name);
            }

            return true;
        }
    }
}
