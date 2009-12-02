/*
 * ROS support library
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
using System.Collections.Generic;
using Castor;

namespace Spica.ROS
{

    /**
     * ROS support class for Aastra.
     *
     * ROS introduced a quite flexible yet lean package management system that allows
     * developers to use readily available modules and contribute new ones. This is facilitated
     * by two concepts: packages and stacks.
     *
     * Packages may be drivers, libraries, utilities, or other pieces of software that are
     * meant to be used by/useful for robotic systems. For each package the relevant information
     * (description, author, license, ...) and build requirements (dependencies) are defined
     * in a so-called manifest file. It must be located in the root directory of that package.
     * Nesting packages is not allowed.
     *
     * Stacks are simply collections of packages, meant to simplify reuse and to ease exchange
     * of functionality. A stack manifest file in the root directory of the stack also delivers
     * the required information. Just as packages, stacks may not be nested.
     *
     * The ROS support library in Aastra is able to locate packages and stacks. Once the manifest
     * files have been loaded, available packages may be bowsed and passed to the Aastra processing
     * engine.
     *
     * ROS furthermore allows to defin messages and services. Messages in ROS are just the same
     * as messages in Spica, except that semantic annotations are not supported and ROS is not
     * aware of inheritance. Services are interfaces for remote procedure calls, i.e. remote
     * methods with a specific signature that accept argument and return an arbitrary result.
     *
     * Both concepts, i.e. messages and services, are mapped onto their Spica counterparts.
     * However, Spica is not yet aware of the concept of remote procedure calls. They are thus
     * mapped onto a simple message exchange pattern for now.
     */
    public class ROS
    {
        // Set to true if ROS was found, false otherwise
        protected bool ros_found = false;

        // Set to the ROS root directory ($ROS_ROOT)
        protected string ros_root = null;
        // Set to the ROS cache timeout if specified, 60s otherwise ($ROS_CACHE_TIMEOUT)
        protected long ros_cache_timeout = 60000;

        // List of discovered ROS stacks
        internal ModuleInfo<Stack> stacks = null;
        public IList<Stack> Stacks
        {
            get { return this.stacks.List; }
        }

        // List of discovered ROS packages
        internal ModuleInfo<Package> packages = null;
        public IList<Package> Packages
        {
            get { return this.packages.List; }
        }

        // Returns true if ROS was found, false otherwise
        public bool Found
        {
            get { return this.ros_found; }
        }

        /**
         * Default constructor.
         *
         * Whether or not ROS was found is reflected by the @p ros_found variable
         * accessible through the @p Found property.
         */
        public ROS()
        {
            this.ros_found = false;

            this.stacks = new ModuleInfo<Stack>();
            this.packages = new ModuleInfo<Package>();

            // Get ROS root directory
            this.ros_root = Environment.GetEnvironmentVariable("ROS_ROOT");

            // Verify that the ROS directory really exists
            if ((this.ros_root == null) || (this.ros_root.Length == 0) ||
                (!Directory.Exists(this.ros_root)))
            {
                Debug.WriteLine("ROS not found, support disabled");
                this.ros_found = false;

                return;
            }

            this.ros_found = true;

            // Get ROS cache timeout
            try
            {
                this.ros_cache_timeout = (long)(Double.Parse(Environment.GetEnvironmentVariable("ROS_CACHE_TIMEOUT")) * 60 * 1000);
            }
            catch (Exception)
            {
                this.ros_cache_timeout = 60000;
            }

            // Also search in the ROS root
            this.stacks.Paths.Add(this.ros_root);
            this.packages.Paths.Add(this.ros_root);

            // Output verbose info for now
            Debug.WriteLine("ROS support enabled");
            Debug.WriteLine("+ Root: {0}", this.ros_root);
            Debug.WriteLine("+ Cache timeout: {0} s", this.ros_cache_timeout / 1000);
        }

        public void Scan()
        {
            if (!Found) return;

            string file = null;

            Debug.WriteLine("ROS: Scanning directories...");

            foreach (string s in this.stacks.Paths)
            {
                Debug.WriteLine("+ Stack: {0}", s);
            }

            foreach (string s in this.packages.Paths)
            {
                Debug.WriteLine("+ Package: {0}", s);
            }

            DoScan(this.stacks);
            DoScan(this.packages);

            Debug.WriteLine("ROS: Resolving dependencies...");

            int no_stacks = 0;
            int no_packages = 0;
            foreach (Stack s in this.stacks.List)
            {
                s.Resolve(this.stacks.List);
                no_stacks++;
            }
            foreach (Package p in this.packages.List)
            {
                p.Resolve(this.packages.List);
                p.ResolveStack(this.stacks.List);
                no_packages++;
            }

            Debug.WriteLine("ROS: Checked {0} stacks/{1} packages", no_stacks, no_packages);
        }

        internal void DoScan<T>(ModuleInfo<T> mi) where T: Module, new()
        {
            mi.Clear();

            // Check if there is a cache file
            string cache_file = Path.Combine(this.ros_root, mi.CacheFile);
            DateTime cache_file_time = File.GetLastWriteTimeUtc(cache_file);

            // Check if the cache is still valid
            long file_age = (DateTime.UtcNow - cache_file_time).Ticks / 10000;
            if (file_age < this.ros_cache_timeout)
            {
                Debug.WriteLine("ROS: Using cache file {0} (age {1} s)", mi.CacheFile, file_age / 1000);

                StreamReader reader = new StreamReader(File.OpenRead(cache_file));

                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    // the environment variables are ignored for now
                    if (line.StartsWith("#")) continue;

                    T instance = new T();
                    mi.List.Add(instance.Init(line) as T);
                }
            }
            else
            {
                // Add all stacks or packages
                foreach (string p in mi.Paths)
                {
                    mi.List.AddRange(GetAllInPath<T>(p));
                }

                // Write cache file
                StreamWriter cache = new StreamWriter(File.Create(cache_file));
                cache.WriteLine("#ROS_ROOT={0}", this.ros_root);
                cache.WriteLine("#ROS_STACK_PATH={0}", mi.ID);
            
                foreach (Module m in mi.List)
                {
                    cache.WriteLine(m.Directory);
                }

                cache.Flush();
                }
        }

        public void AddPackagePath(string path)
        {
            this.packages.Paths.Add(path);
        }

        protected IList<string> GetEnvPaths(string paths)
        {
            if (paths == null) return new string[0];

            // TODO: UNIX-specific
            return paths.Split(new char[] { ':' },
                               StringSplitOptions.RemoveEmptyEntries);
        }

        /**
         * Returns a list of stacks or packages. This method recursively scans 
         * the given directory (@p start) and tries to find all ROS stacks and packages,
         * depending on what type is searched for (@p T).
         * @start The directory to start at
         * @return A list of stacks or packages that are contained in the given directory
         */
        protected IList<T> GetAllInPath<T>(string start) where T: Module, new()
        {
            T instance = new T();

            // Check the start directory
            string[] files = Directory.GetFiles(start, instance.ManifestFile);
            if (files.Length > 0)
            {
                return new T[] { instance.Init(start) as T };
            }

            List<T> result = new List<T>();
            // Check all subdirectories only if there is no description file found
            // (packages and stacks cannot be nested)
            foreach (string d in Directory.GetDirectories(start))
            {
                result.AddRange(GetAllInPath<T>(d));
            }

            return result;
        }
    }
}

