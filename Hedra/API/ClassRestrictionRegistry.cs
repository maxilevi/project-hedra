using System.Linq;
using Hedra.Engine.ClassSystem;

namespace Hedra.API
{
    public class ClassRestrictionRegistry : Registry<Class, string>
    {
        protected override void MeetsRequirements(Class Key, string Value)
        {
        }

        protected override void DoAdd(Class Key, string Value)
        {
            var key = ClassDesign.FromString(Key.ToString()).GetType();
            var values = new[] { Value };
            if (RestrictionsFactory.Instance.Has(key))
            {
                values = values.Concat(RestrictionsFactory.Instance.Get(key)).ToArray();
                RestrictionsFactory.Instance.Unregister(key);
            }

            RestrictionsFactory.Instance.Register(key, values);
        }

        protected override void DoRemove(Class Key, string Value)
        {
            var key = ClassDesign.FromString(Key.ToString()).GetType();
            var values = RestrictionsFactory.Instance.Get(key);
            values = values.Where(V => V != Value).ToArray();
            if (values.Length == 0)
                RestrictionsFactory.Instance.Unregister(ClassDesign.FromString(Key.ToString()).GetType());
            else
                RestrictionsFactory.Instance.Register(ClassDesign.FromString(Key.ToString()).GetType(), values);
        }

        public override void Unregister()
        {
            RestrictionsFactory.Instance.Clear();
        }

        public override void Add(Class Key, string Value)
        {
            DoAdd(Key, Value);
        }
    }
}