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

        public T Execute<T>(string Function, params object[] Params)
        {
            return Interpreter.GetFunction(_name, Function).Invoke<T>(Params);
        }
        
        public void Execute(string Function, params object[] Params)
        {
            Interpreter.GetFunction(_name, Function).Invoke(Params);
        }
        
        public T Get<T>(string Member)
        {
            return Interpreter.GetMember<T>(_name, Member);
        }
    }
}