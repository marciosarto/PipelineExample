using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Rhino.Display;
using Rhino.DocObjects;
using Font = System.Drawing.Font;

namespace PrintPipelineExample
{

    public class Pipeline
    {

        public static Pipeline ActivePipeline { get; set; }

        public Pipeline()
        {
            this.Enabled = true;
            
        }

        public List<Rectangle3d> rectangle = new List<Rectangle3d>();

        public List<string> text = new List<string>();

        private bool enabled;

        /// <summary>
        /// Gets or sets the enabled state of the Geometry Pipeline
        /// </summary>
        internal bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;

                    if (enabled)
                    {
                        DisplayPipeline.PostDrawObjects += PostDrawObjects;
                    }
                    else
                    {
                        DisplayPipeline.PostDrawObjects -= PostDrawObjects;
                    }
                }
            }
        }
        protected void PostDrawObjects(object sender, DrawEventArgs e)
        {
            var textandRect = text.Zip(rectangle, (t, r) => new { Text = t, Rectangle = r });
            foreach (var tr in textandRect)
            {
                e.Display.Draw3dText(tr.Text, Color.Black, new Plane(tr.Rectangle.Center, Vector3d.ZAxis), 40, "Arial", false, false,
                    TextHorizontalAlignment.Center, TextVerticalAlignment.Middle);
                e.Display.DrawCurve(tr.Rectangle.ToNurbsCurve(), Color.Black, 1);
            }
        }


    }

    public class PrintPipelineExampleCommand : Command
    {
        public PrintPipelineExampleCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PrintPipelineExampleCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "PrintPipeline";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Pipeline.ActivePipeline = new Pipeline();
            Curve curve;
            var go = new GetObject();
            go.SetCommandPrompt("Select shape");
            go.GeometryFilter = ObjectType.Curve;
            go.SubObjectSelect = false;
            go.EnableClearObjectsOnEntry(false);
            go.EnableUnselectObjectsOnExit(false);
            go.DeselectAllBeforePostSelect = false;
            go.GetMultiple(1, 1000);
            int i= 1;
            foreach (ObjRef obj in go.Objects())
            {
                Curve cv = obj.Geometry() as Curve;
                AreaMassProperties prop = AreaMassProperties.Compute(cv);
                Plane centroidPlane = new Plane(prop.Centroid, Vector3d.ZAxis);
                Rectangle3d rect = new Rectangle3d(centroidPlane, 200, 50);
                string text = "Shape " + i;
                i++;
                Pipeline.ActivePipeline.rectangle.Add(rect);
                Pipeline.ActivePipeline.text.Add(text);
            }

            // ---
            return Result.Success;
        }
    }
}
