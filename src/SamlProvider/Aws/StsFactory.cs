using System.Threading.Tasks;

using Amazon.SecurityToken;

namespace Cythral.CloudFormation.Resources.Aws
{
    public class StsFactory
    {
        public virtual Task<IAmazonSecurityTokenService> Create()
        {
            return Task.FromResult((IAmazonSecurityTokenService)new AmazonSecurityTokenServiceClient());
        }
    }
}