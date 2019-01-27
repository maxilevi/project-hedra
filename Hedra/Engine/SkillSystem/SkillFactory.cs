using System;
using Hedra.Engine.Core;

namespace Hedra.Engine.SkillSystem
{
    public class SkillFactory : TypeFactory<SkillFactory>
    {
        public Type Get(string Name)
        {
            return this[Name];
        }
        
        public Type[] GetAll()
        {
            return base.GetAll();
        }
    }
}