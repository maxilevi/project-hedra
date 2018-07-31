using System.Text;
using System.Text.RegularExpressions;
using Hedra.Engine.Management;

namespace Hedra.Engine.Testing.CodePolicies
{
    public class ShaderCodePolicy : BaseTest
    {
        private string _shaderCode;

        public override void Setup()
        {
            _shaderCode = AssetManager.ShaderCode;
        }

        [TestMethod]
        public void TestNumbersInDontUseSuffixLiterals()
        {
            var regex = new Regex(@".*[\d]+f.*|.*[\d]+F.*|.*[\d]+d.*");
            var matches = regex.Matches(_shaderCode);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                this.AssertTrue(false, "Invalid suffix literal at line |"+match.Value);
            }
        }

        [TestMethod]
        public void TestNumbersUseDecimals()
        {
            var regex = new Regex(@"(?i)textureSize.*|int.*|layout.*|\/\/.*|#.*|\[.+]|(?<=[a-zA-Z])[\d]+|[\d]+(?=\.)|\.[\d]+|([\d]+.*)");
            var matches = regex.Matches(_shaderCode);
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i].Groups[1];
                this.AssertEqual(match.Value, string.Empty, "Invalid int at line : " + match.Value);
            }
        }
    }
}
