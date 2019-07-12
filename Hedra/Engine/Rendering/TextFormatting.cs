using Hedra.Engine.Scripting;

namespace Hedra.Engine.Rendering
{
    public static class TextFormatting
    {
        private const string Library = "TextDisplay.py";
        public static readonly string Red = Interpreter.GetMember<string>(Library, "RED");
        public static readonly string Gray = Interpreter.GetMember<string>(Library, "GRAY");
        public static readonly string Violet = Interpreter.GetMember<string>(Library, "VIOLET");
        public static readonly string Blue = Interpreter.GetMember<string>(Library, "BLUE");
        public static readonly string Cyan = Interpreter.GetMember<string>(Library, "CYAN");
        public static readonly string White = Interpreter.GetMember<string>(Library, "WHITE");
        public static readonly string Pastel = Interpreter.GetMember<string>(Library, "PASTEL");
        public static readonly string Green = Interpreter.GetMember<string>(Library, "GREEN");
        public static readonly string Orange = Interpreter.GetMember<string>(Library, "ORANGE");
        public static readonly string Black = Interpreter.GetMember<string>(Library, "BLACK");
        public static readonly string Gold = Interpreter.GetMember<string>(Library, "GOLD");
        public static readonly string Yellow = Interpreter.GetMember<string>(Library, "YELLOW");
        public static readonly string Bold = Interpreter.GetMember<string>(Library, "BOLD");
        public static readonly string Normal = Interpreter.GetMember<string>(Library, "NORMAL");
        public static readonly string Smaller = Interpreter.GetMember<string>(Library, "SMALLER");
        public static readonly string Bigger = Interpreter.GetMember<string>(Library, "BIGGER");
        public static readonly string Caps = Interpreter.GetMember<string>(Library, "CAPS");
    }
}