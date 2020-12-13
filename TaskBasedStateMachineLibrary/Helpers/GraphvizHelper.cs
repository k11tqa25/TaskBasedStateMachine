using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace TaskBasedStateMachineLibrary
{
    /// <summary>
    /// A static helper class for running the dot.exe to create the flow chart. 
    /// </summary>
    public static class GraphvizHelper
    {
        /// <summary>
        /// Draw the graph as an image.
        /// See more details in Graphviz website at: https://graphviz.org/about/ 
        /// </summary>
        /// <param name="dot">The string that can interpret by dot.exe. See more details in Graphviz website at: https://graphviz.org/about/ </param>
        /// <param name="saveImageFilename">Save the image to this filename. Leave it empty if you don't want to save the image.</param>
        /// <returns>Returns a <see cref="Image"/></returns>
        public static Image Run(string dot, string saveImageFilename)
        {
            try
            {
                string executable = Directory.GetCurrentDirectory() +@"\external\dot.exe";
                string defaultOutput = Directory.GetCurrentDirectory() + @"\external\__tempgraph";
                File.WriteAllText(defaultOutput, dot);

                System.Diagnostics.Process process = new System.Diagnostics.Process();

                // Stop the process from opening a new window
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                // Setup executable and parameters
                process.StartInfo.FileName = executable;
                process.StartInfo.Arguments = string.Format(@"{0} -Tjpg -O", defaultOutput);

                // Go
                process.Start();
                // and wait dot.exe to complete and exit
                process.WaitForExit();
                Image image;
                using (Stream bmpStream = System.IO.File.Open(defaultOutput + ".jpg", System.IO.FileMode.Open))
                {
                    // Read the temp image from source
                    image = Image.FromStream(bmpStream);

                    // Save an additional image if user assigns one.
                    if (saveImageFilename != string.Empty)
                    {
                        try
                        {
                            // This will throw an exception if the path is invalid
                            saveImageFilename = Path.GetFullPath(saveImageFilename);
                            image.Save(saveImageFilename);
                        }
                        catch (Exception ex) { OnErrorOccurs.Invoke((nameof(GraphvizHelper), ex)); }
                    }
                }
                File.Delete(defaultOutput);
                File.Delete(defaultOutput + ".jpg");
                return image;
            }
            catch (Exception ex)
            {
                OnErrorOccurs.Invoke((nameof(GraphvizHelper), ex));
                return null;
            }
        }

        /// <summary>
        /// Renders an image directly
        /// </summary>
        public static class Graphviz
        {
            /// <summary>
            ///  The location of the gvc.dll
            /// </summary>
            public const string LIB_GVC = @".\external\gvc.dll";

            /// <summary>
            /// The location of the cgraph.dll
            /// </summary>
            public const string LIB_GRAPH = @".\external\cgraph.dll";

            /// <summary>
            /// Is render successful?
            /// </summary>
            public const int SUCCESS = 0;

            /// 
            /// Creates a new Graphviz context.
            /// 

            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr gvContext();

            /// 
            /// Releases a context's resources.
            /// 
            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern int gvFreeContext(IntPtr gvc);

            /// 
            /// Reads a graph from a string.
            /// 
            [DllImport(LIB_GRAPH, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr agmemread(string data);


            /// 
            /// Releases the resources used by a graph.
            /// 
            [DllImport(LIB_GRAPH, CallingConvention = CallingConvention.Cdecl)]
            public static extern void agclose(IntPtr g);

            /// 
            /// Applies a layout to a graph using the given engine.
            /// 
            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern int gvLayout(IntPtr gvc, IntPtr g, string engine);


            /// 
            /// Releases the resources used by a layout.
            /// 
            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern int gvFreeLayout(IntPtr gvc, IntPtr g);

            /// 
            /// Renders a graph to a file.
            /// 
            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern int gvRenderFilename(IntPtr gvc, IntPtr g,
                  string format, string fileName);

            /// 
            /// Renders a graph in memory.
            /// 
            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern int gvRenderData(IntPtr gvc, IntPtr g,
                  string format, out IntPtr result, out int length);

            /// 
            /// Release render resources.
            /// 
            [DllImport(LIB_GVC, CallingConvention = CallingConvention.Cdecl)]
            public static extern int gvFreeRenderData(IntPtr result);

            /// <summary>
            /// Render the source to the image
            /// </summary>
            /// <param name="source">The string to run the dot.exe.</param>
            /// <param name="format">The image output format.</param>
            /// <returns></returns>
            public static Image RenderImage(string source, string format)
            {
                try
                {
                    // Create a Graphviz context
                    IntPtr gvc = gvContext();
                    if (gvc == IntPtr.Zero)
                        throw new Exception("Failed to create Graphviz context.");

                    // Load the DOT data into a graph
                    IntPtr g = agmemread(source);
                    if (g == IntPtr.Zero)
                        throw new Exception("Failed to create graph from source. Check for syntax errors.");

                    // Apply a layout
                    if (gvLayout(gvc, g, "dot") != SUCCESS)
                        throw new Exception("Layout failed.");

                    IntPtr result;
                    int length;

                    // Render the graph
                    if (gvRenderData(gvc, g, format, out result, out length) != SUCCESS)
                        throw new Exception("Render failed.");

                    // Create an array to hold the rendered graph
                    byte[] bytes = new byte[length];

                    // Copy the image from the IntPtr
                    Marshal.Copy(result, bytes, 0, length);

                    // Free up the resources
                    gvFreeRenderData(result);
                    gvFreeLayout(gvc, g);
                    agclose(g);
                    gvFreeContext(gvc);
                    using (MemoryStream stream = new MemoryStream(bytes))
                    {
                        return Image.FromStream(stream);
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccurs.Invoke((nameof(GraphvizHelper), ex));
                    return null;
                }
            }
        }

        /// <summary>
        /// Fires whenever an error occurs.
        /// </summary>
        public static event Action<(object sender, Exception ex)> OnErrorOccurs = (detail) => { };
    }
}
