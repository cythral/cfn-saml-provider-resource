using System.Threading.Tasks;

using Amazon.IdentityManagement;
using Amazon.SecurityToken.Model;

namespace Cythral.CloudFormation.Resources.Aws
{
    public class IamFactory
    {
        private StsFactory stsFactory = new StsFactory();

        public virtual async Task<IAmazonIdentityManagementService> Create(string? roleArn = null)
        {
            if (roleArn != null)
            {
                var client = await stsFactory.Create();
                var response = await client.AssumeRoleAsync(new AssumeRoleRequest
                {
                    RoleArn = roleArn,
                    RoleSessionName = "saml-provider-iam-ops"
                });

                return new AmazonIdentityManagementServiceClient(response.Credentials);
            }

            return (IAmazonIdentityManagementService)new AmazonIdentityManagementServiceClient();
        }
    }
}