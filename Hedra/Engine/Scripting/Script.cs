namespace Hedra.Engine.Scripting
{
    public class Script
    {
        private readonly string _name;

        public Script(string Name)
        {
            _name = Name;
        }

        public Function Get(string Function)
        {
            return Interpreter.GetFunction(_name, Function);
        }

        public T Get<T>(string Member)
        {
            return Interpreter.GetMember<T>(_name, Member);
        }
    }
}