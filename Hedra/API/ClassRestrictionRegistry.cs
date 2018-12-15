using System.Collections.Generic;
using Hedra.Engine.ClassSystem;

namespace Hedra.API
{
    public class ClassRestrictionRegistry : Registry<Class, string>
    {
        private static readonly List<string> Restrictions = new List<string>();
        
        protected override void MeetsRequirements(Class Key, string Value)
        {
        }

        protected override void DoAdd(Class Key, string Value)
        {
            Restrictions.Add(Value);
            RestrictionsFactory.Instance.Register(ClassDesign.FromString(Key.ToString()).GetType(), Restrictions.ToArray());
        }

        protected override void DoRemove(Class Key, string Value)
        {
            Restrictions.Remove(Value);
            if(Restrictions.Count == 0)
                RestrictionsFactory.Instance.Unregister(ClassDesign.FromString(Key.ToString()).GetType());
            else
                RestrictionsFactory.Instance.Register(ClassDesign.FromString(Key.ToString()).GetType(), Restrictions.ToArray());
        }
    }
}