using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ImageComposeEditorAutomation
{
    public enum CameraMotion
    {
        [Description("Auto-detect")]
        autoDetect,
        [Description("Planar motion")]
        planarMotion,
        [Description("Planar motion with skew")]
        planarMotionWithSkew,
        [Description("Planar motion with perspective")]
        planarMotionWithPerspective,
        [Description("Rotating motion")]
        rotatingMotion
    }

    public enum Corner
    {
        [Description("Top Left")]
        topLeft,
        [Description("Top Right")]
        topRight,
        [Description("Bottom Left")]
        bottomLeft,
        [Description("Bottom Right")]
        bottomRight
    }

    public enum Direction
    {
        [Description("Left")]
        left,
        [Description("Right")]
        right,
        [Description("Bottom")]
        bottom,
        [Description("Top")]
        top
    }

    public enum ImageOrder
    {
        [Description("Serpentine")]
        serpentine,
        [Description("Zigzag")]
        zigzag
    }

    public enum AngularRange
    {
        [Description("Less than 360")]
        less360,
        [Description("360 horiz")]
        horiz,
        [Description("360 vert")]
        vert
    
    }

    public class BaseOptions 
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "save", Required = false, HelpText = "Save project file.")]
        public bool? Save { get; set; }

        [Option('m', "motion", Required = false, HelpText = "Set camera motion.")]
        public CameraMotion Motion { get; set; }

    }

    [Verb("compose", HelpText = "compose <file1> <file2> <file3>....  Stich file1 fil2,... ")]
    public class ComposeOptions : BaseOptions
    {
        [Value(0)]
        public IEnumerable<string> Images { get; set; }

    }
    
    public class ProcessBaseOptions : BaseOptions
    {
        [Value(0)]
        public int Num { get; set; }

        [Value(1)]
        public string Extension { get; set; }
        [Value(2)]
        public string Folder { get; set; }

    }

     [Verb("process", HelpText = "process <num> <ext> <folder>. Process all <ext=*.JPG> files in <folder=current> in groups of <num=3>")]
    public class ProcessOptions : ProcessBaseOptions
    {
       

    }

    [Verb("structure", HelpText = "process <num> <ext> <folder>. Process all <ext=*.JPG> files in <folder=current> in groups of <num=3>")]
    public class StructurePanoramaOptions : ProcessBaseOptions
    {
        [Option('i', "initial-corner", Required = false, HelpText = "Initial corner: topLeft (default), topRight, bottomLeft, bottomRight", Default = Corner.topLeft)]
        public Corner InitialCorner { get; set; }

        [Option('r', "rows", Required = false, HelpText = "Number of rows. If defined the direction will be down (if intial corner is top) or up (if initial corner is bottom)", Default = null)]
        public int? Rows { get; set; }

        [Option('c', "columns", Required = false, HelpText = "Number of columns. If defined the direction will be right (if intial corner is left) or left (if initial corner is right)", Default = null)]
        public int? Columns { get; set; }

        [Option('o', "order", Required = false, HelpText = "serpentine, zigzag", Default = ImageOrder.serpentine)]
        public ImageOrder Order { get; set; }

        [Option('g', "angular-range", Required = false, HelpText = "Angular range: less360, horiz, vert", Default = AngularRange.less360)]
        public AngularRange AngularRange { get; set; }

        [Option('h', "horizontal-overlap", Required = false, HelpText = "Horizontal overlap", Default = null)]
        public int? HorizontalOverlap { get; set; }

        [Option('v', "vertical-overlap", Required = false, HelpText = "Vertical overlap.", Default = null)]
        public int? VerticalOverlap { get; set; }

        [Option('s', "search-radious", Required = false, HelpText = "Search Radious.", Default = 10)]
        public int SearchRadious { get; set; }

        [Option('a', "auto-overlap", Required = false, HelpText = "Set camera motion.", Default = true)]
        public bool AutoOverlap { get; set; }
    }
}